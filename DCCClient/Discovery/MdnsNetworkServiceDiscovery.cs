using System.Collections.Concurrent;
using System.Net.Sockets;
using DCCClient.Helpers;
using Makaretu.Dns;

namespace DCCClient.Discovery;

public sealed class MdnsNetworkServiceDiscovery : INetworkServiceDiscovery {
    private readonly ConcurrentDictionary<string, DiscoveredService> _discoveredServicesCache = new();
    private readonly Lock _initializationLock = new();
    private TaskCompletionSource<bool>? _discoveryCompletionSource;
    private MulticastService? _mdns;
    private ServiceDiscovery? _sd;

    public async Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                              TimeSpan timeout,
                                                                              CancellationToken cancellationToken = default) {
        return await DiscoverServicesAsync(serviceType, "", timeout, cancellationToken);
    }

    public async Task<IResult<List<DiscoveredService>>> DiscoverServicesAsync(string serviceType,
                                                                              string subType,
                                                                              TimeSpan timeout,
                                                                              CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(serviceType)) throw new ArgumentNullException(nameof(serviceType));

        var queryServiceType = serviceType.Trim();
        var querySubType = subType.Trim().ToLower();

        lock (_initializationLock) _discoveredServicesCache.Clear();
        _discoveryCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        StartMdnsListener();

        if (_sd == null || _mdns == null) { // Also check _mdns
            return Result<List<DiscoveredService>>.Fail("MdnsNetworkServiceDiscovery: ServiceDiscovery component or MulticastService failed to initialize.");
        }

        try {
            _sd.QueryServiceInstances(queryServiceType);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var delayTask = Task.Delay(timeout, linkedCts.Token);

            // We don't strictly need _discoveryCompletionSource.Task if we're just waiting for the timeout
            // The events will populate the cache in the background.
            // await Task.WhenAny(_discoveryCompletionSource.Task, delayTask);
            await delayTask; // Simply wait for the timeout or cancellation

            if (cancellationToken.IsCancellationRequested) {
                return Result<List<DiscoveredService>>.Fail("MdnsNetworkServiceDiscovery: Discovery cancelled by token.");
            }
        } catch (OperationCanceledException) {
            return Result<List<DiscoveredService>>.Fail("MdnsNetworkServiceDiscovery: Discovery operation was cancelled.");
        } catch (Exception ex) {
            return Result<List<DiscoveredService>>.Fail("MdnsNetworkServiceDiscovery: Error during service discovery", ex);
        }

        var values = _discoveredServicesCache.Values
                                             .Where(s => (s.ServiceType.Equals(queryServiceType, StringComparison.OrdinalIgnoreCase) ||
                                                          s.ServiceType.Equals(queryServiceType + ".local", StringComparison.OrdinalIgnoreCase)) &&
                                                         (querySubType == "" || (s?.HostName?.ToLower()?.Contains(querySubType) ?? false)) &&
                                                         !string.IsNullOrEmpty(s.HostName) &&
                                                         s.Port > 0 &&
                                                         s.Addresses.Any())
                                             .ToList();

        return values.Count > 0 ? Result<List<DiscoveredService>>.Ok(values) : Result<List<DiscoveredService>>.Fail("No available services found.");
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void EnsureComponentsInitialized() {
        lock (_initializationLock) {
            if (_mdns == null || _sd == null) {
                _mdns = new MulticastService {
                    IgnoreDuplicateMessages = true
                };
                _sd = new ServiceDiscovery(_mdns);
                _sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
                _mdns.AnswerReceived += OnAnswerReceived; // Important for getting all record types
            }
        }
    }

    private void StartMdnsListener() {
        EnsureComponentsInitialized();

        // The Start() method of Makaretu.Dns.MulticastService is idempotent,
        // meaning it's safe to call even if already started.
        if (_mdns != null) {
            try {
                _mdns.Start(); // Safe to call
            } catch (SocketException se) when (se.SocketErrorCode == SocketError.AddressAlreadyInUse) {
                Console.WriteLine($"MdnsNetworkServiceDiscovery: mDNS port (5353) is already in use. Assuming another mDNS service is running. {se.Message}");
            } catch (Exception ex) {
                Console.WriteLine($"MdnsNetworkServiceDiscovery: Error starting mDNS listener: {ex.Message}");
                throw; // Rethrow if it's a critical failure
            }
        }
    }

    private void OnServiceInstanceDiscovered(object? sender, ServiceInstanceDiscoveryEventArgs e) {
        ProcessDnsMessage(e.Message, e.ServiceInstanceName);
    }

    private void OnAnswerReceived(object? sender, MessageEventArgs e) {
        if (e.Message.IsQuery) return; // We only care about responses
        ProcessDnsMessage(e.Message);
    }

    private void ProcessDnsMessage(Message message, DomainName? initialInstanceName = null) {
        var allRecords = message.Answers.Union(message.AdditionalRecords).ToList();

        // bool serviceUpdated = false; // This flag wasn't used to control logic flow, can be removed if not needed for other purposes.

        // Process PTR records first to identify service instances
        foreach (var ptrRecord in allRecords.OfType<PTRRecord>()) {
            var instanceName = ptrRecord.DomainName; // This is the service instance name
            var service = _discoveredServicesCache.GetOrAdd(instanceName.ToString(), key => {
                // serviceUpdated = true;
                var newService = new DiscoveredService { InstanceName = key };
                var labels = instanceName.Labels.ToList();
                if (labels.Any()) newService.FriendlyName = labels[0];
                if (labels.Count > 1) newService.ServiceType = string.Join(".", labels.Skip(1)).TrimEnd('.');
                return newService;
            });

            if (string.IsNullOrEmpty(service.ServiceType)) {
                // The PTR record's Name field is the service type (e.g., _http._tcp.local)
                service.ServiceType = ptrRecord.Name.ToString().TrimEnd('.');
            }
        }

        // Process SRV, TXT, A, AAAA records and associate them with known instances
        foreach (var record in allRecords) {
            DiscoveredService? serviceToUpdate = null;

            // Prioritize matching record.Name directly to an instance name in the cache
            if (_discoveredServicesCache.TryGetValue(record.Name.ToString(), out var directMatchService)) {
                serviceToUpdate = directMatchService;
            }

            // If initialInstanceName is provided (from ServiceInstanceDiscovered event), use it if it matches record.Name
            else if (initialInstanceName != null && record.Name == initialInstanceName && _discoveredServicesCache.TryGetValue(initialInstanceName.ToString(), out var initialServiceMatch)) {
                serviceToUpdate = initialServiceMatch;
            }

            // Fallback: Record name might be a hostname from an SRV record, try to find a service by HostName
            else {
                serviceToUpdate = _discoveredServicesCache.Values.FirstOrDefault(s => !string.IsNullOrEmpty(s.HostName) && s.HostName.Equals(record.Name.ToString().TrimEnd('.'), StringComparison.OrdinalIgnoreCase));
            }

            // If initialInstanceName is provided and we haven't found a serviceToUpdate yet,
            // and the record.Name matches the HostName of the service associated with initialInstanceName
            if (serviceToUpdate == null && initialInstanceName != null && _discoveredServicesCache.TryGetValue(initialInstanceName.ToString(), out var initialServiceForHostNameMatch)) {
                if (!string.IsNullOrEmpty(initialServiceForHostNameMatch.HostName) && record.Name.ToString().TrimEnd('.') == initialServiceForHostNameMatch.HostName) {
                    serviceToUpdate = initialServiceForHostNameMatch;
                }
            }

            if (serviceToUpdate == null) continue;

            var currentServiceUpdated = false;
            switch (record) {
            case SRVRecord srv:
                if (serviceToUpdate.InstanceName == srv.Name.ToString()) {
                    if (serviceToUpdate.Port != srv.Port || serviceToUpdate.HostName != srv.Target.ToString().TrimEnd('.')) {
                        serviceToUpdate.Port = srv.Port;
                        serviceToUpdate.HostName = srv.Target.ToString().TrimEnd('.');
                        currentServiceUpdated = true;
                    }
                }
                break;

            case TXTRecord txt:
                if (serviceToUpdate.InstanceName == txt.Name.ToString()) {
                    foreach (var txtString in txt.Strings) {
                        var parts = txtString.Split(new[] { '=' }, 2);
                        if (parts.Length == 2 && !string.IsNullOrEmpty(parts[0])) {
                            if (!serviceToUpdate.TxtRecords.TryGetValue(parts[0], out var existingValue) || existingValue != parts[1]) {
                                serviceToUpdate.TxtRecords[parts[0]] = parts[1];
                                currentServiceUpdated = true;
                            }
                        } else if (!string.IsNullOrEmpty(txtString) && !serviceToUpdate.TxtRecords.ContainsKey(txtString)) {
                            serviceToUpdate.TxtRecords[txtString] = string.Empty; // Valueless key
                            currentServiceUpdated = true;
                        }
                    }
                }
                break;

            case ARecord a:
                var servicesForA = _discoveredServicesCache.Values.Where(s => !string.IsNullOrEmpty(s.HostName) && s.HostName.Equals(a.Name.ToString().TrimEnd('.'), StringComparison.OrdinalIgnoreCase)).ToList();
                if (serviceToUpdate != null && !string.IsNullOrEmpty(serviceToUpdate.HostName) && serviceToUpdate.HostName.Equals(a.Name.ToString().TrimEnd('.'), StringComparison.OrdinalIgnoreCase) && !servicesForA.Contains(serviceToUpdate)) {
                    servicesForA.Add(serviceToUpdate); // Ensure current serviceToUpdate is considered if its HostName matches
                }
                foreach (var svc in servicesForA.Distinct()) { // Use Distinct in case serviceToUpdate was already in servicesForA
                    if (!svc.Addresses.Contains(a.Address)) {
                        svc.Addresses.Add(a.Address);
                        currentServiceUpdated = true;
                    }
                }
                break;

            case AAAARecord aaaa:
                var servicesForAAAA = _discoveredServicesCache.Values.Where(s => !string.IsNullOrEmpty(s.HostName) && s.HostName.Equals(aaaa.Name.ToString().TrimEnd('.'), StringComparison.OrdinalIgnoreCase)).ToList();
                if (serviceToUpdate != null && !string.IsNullOrEmpty(serviceToUpdate.HostName) && serviceToUpdate.HostName.Equals(aaaa.Name.ToString().TrimEnd('.'), StringComparison.OrdinalIgnoreCase) && !servicesForAAAA.Contains(serviceToUpdate)) {
                    servicesForAAAA.Add(serviceToUpdate);
                }
                foreach (var svc in servicesForAAAA.Distinct()) {
                    if (!svc.Addresses.Contains(aaaa.Address)) {
                        svc.Addresses.Add(aaaa.Address);
                        currentServiceUpdated = true;
                    }
                }
                break;
            }
            if (currentServiceUpdated) {
                currentServiceUpdated = false;
            }
        }

        // The serviceUpdated flag wasn't driving any specific logic after the loop,
        // so its direct utility here is minimal unless used for more granular TaskCompletionSource logic.
    }

    private void Dispose(bool disposing) {
        if (disposing) {
            lock (_initializationLock) {
                if (_sd != null) {
                    _sd.ServiceInstanceDiscovered -= OnServiceInstanceDiscovered;
                }
                if (_mdns != null) {
                    _mdns.AnswerReceived -= OnAnswerReceived;

                    // The Stop() method of Makaretu.Dns.MulticastService is idempotent
                    try {
                        _mdns.Stop(); // Safe to call
                    } catch (Exception ex) {
                        Console.WriteLine($"MdnsNetworkServiceDiscovery: Error stopping mDNS on dispose: {ex.Message}");
                    }
                    _mdns.Dispose();
                }
                _mdns = null;
                _sd = null;
            }
        }
    }
}