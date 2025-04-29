using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Discovers network services using Multicast DNS (mDNS)
    /// </summary>
    public class NetworkServiceDiscovery : INetworkServiceDiscovery
    {
        private readonly MulticastService _mdns;
        private readonly ServiceDiscovery _sd;
        private readonly List<DiscoveredService> _discoveredServices = new();
        private readonly object _lock = new();
        private bool _isDisposed;

        /// <summary>
        /// Creates a new instance of the NetworkServiceDiscovery class
        /// </summary>
        public NetworkServiceDiscovery()
        {
            _mdns = new MulticastService();
            _sd = new ServiceDiscovery(_mdns);
            
            // Handle service discovery events
            _sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            
            // Start the discovery service
            _mdns.Start();
        }

        /// <summary>
        /// Handles service instance discovery events
        /// </summary>
        private void OnServiceInstanceDiscovered(object sender, ServiceInstanceDiscoveryEventArgs e)
        {
            lock (_lock)
            {
                // Create a new discovered service
                var service = new DiscoveredService
                {
                    InstanceName = e.ServiceInstanceName.ToString(),
                    HostName = e.ServiceInstanceName.Labels.Count > 0 ? e.ServiceInstanceName.Labels[0] : "unknown service name",
                    Addresses = [e.RemoteEndPoint.Address],
                    Port = e.RemoteEndPoint.Port,
                    ServiceType = string.Join(".", e.ServiceInstanceName.Labels.Skip(1))
                };
                //Console.WriteLine($"Found service: {e.ToString()} => {service.InstanceName}:{service.HostName}:{service.Port}:{service.Address}:{service.ServiceType}");
                
                // Extract IP addresses from additional records
                if (e.Message != null)
                {
                    foreach (var record in e.Message.AdditionalRecords)
                    {
                        var foundHost = e.ServiceInstanceName.Labels.Count > 0 ? e.ServiceInstanceName.Labels[0] : "unknown service name";
                        //Console.WriteLine($"Found record: {record} => {foundHost}");
                        if (record is ARecord aRecord && aRecord.Name == foundHost) {
                            service.Addresses.Add(aRecord.Address);
                        }
                        else if (record is AAAARecord aaaaRecord && aaaaRecord.Name == foundHost) {
                            service.Addresses.Add(aaaaRecord.Address);
                        }
                        else if (record is TXTRecord txtRecord) {
                            service.TxtRecords.AddRange(txtRecord.Strings);
                        }
                    }
                }
                
                // Add to discovered services if not already present
                if (!_discoveredServices.Any(s => s.InstanceName == service.InstanceName))
                {
                    _discoveredServices.Add(service);
                }
            }
        }

        /// <summary>
        /// Discovers services of the specified type on the local network
        /// </summary>
        /// <param name="serviceType">The service type to discover (e.g., "_http._tcp.local")</param>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered service information</returns>
        public async Task<List<DiscoveredService>> DiscoverServicesAsync(string serviceType, int timeoutSeconds = 5, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            
            lock (_lock)
            {
                _discoveredServices.Clear();
            }
            
            // Query for service instances
            _sd.QueryServiceInstances(serviceType);
            
            try
            {
                // Wait for responses
                await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Return what we have so far if canceled
            }
            
            // Return discovered services
            lock (_lock)
            {
                return _discoveredServices
                    .Where(s => s.ServiceType == serviceType)
                    .ToList();
            }
        }

        /// <summary>
        /// Disposes resources used by the NetworkServiceDiscovery
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _sd.ServiceInstanceDiscovered -= OnServiceInstanceDiscovered;
            _mdns.Stop();
            
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
        
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(NetworkServiceDiscovery));
            }
        }
    }
}
