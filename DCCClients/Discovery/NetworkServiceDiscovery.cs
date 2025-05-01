using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DCCClients.Discovery {
    /// <summary>
    /// Discovers network services using Multicast DNS (mDNS)
    /// </summary>
    public class NetworkServiceDiscovery : INetworkServiceDiscovery {
        private readonly MulticastService _mdns;
        private readonly ServiceDiscovery _sd;
        private readonly Dictionary<string,DiscoveredService> _discoveredServices = [];
        private readonly Lock _lock = new();
        private bool _isDisposed;
        private int _count = 0;

        /// <summary>
        /// Creates a new instance of the NetworkServiceDiscovery class
        /// </summary>
        public NetworkServiceDiscovery() {
            _mdns = new MulticastService();
            _sd = new ServiceDiscovery(_mdns);
            _sd.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            _mdns.Start();
            _count = 0;
        }

        /// <summary>
        /// Handles service instance discovery events
        /// </summary>
        private void OnServiceInstanceDiscovered(object? sender, ServiceInstanceDiscoveryEventArgs e) {
            lock (_lock) {
                if (e.Message is {   } message && e.RemoteEndPoint.AddressFamily == AddressFamily.InterNetwork) {
                    foreach (var answer in message.Answers) {
                        if (answer is ARecord { Type: DnsType.A }) {
                            var address = $"{e.RemoteEndPoint.Address}:{e.RemoteEndPoint.Port}";
                            if (!_discoveredServices.ContainsKey(address)) {
                                var service = new DiscoveredService {
                                    InstanceName = e.ServiceInstanceName.ToString(),
                                    HostName = e.ServiceInstanceName.Labels.Count > 0 ? e.ServiceInstanceName.Labels[0] : "unknown service name",
                                    Addresses = [e.RemoteEndPoint.Address],
                                    Port = e.RemoteEndPoint.Port,
                                    ServiceType = string.Join(".", e.ServiceInstanceName.Labels.Skip(1))
                                };
                                _discoveredServices.Add(address,service);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Discovers services of the specified type on the local network
        /// </summary>
        /// <param name="servicename">Name of the service to find</param>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered service information</returns>
        public async Task<List<DiscoveredService>> DiscoverServicesAsync(string serviceName, int timeoutSeconds = 5, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

            lock (_lock) {
                _discoveredServices.Clear();
            }
            _sd.QueryAllServices();
            
            try {
                await Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);
            } catch (TaskCanceledException) {
                /* just exit */
            }

            lock (_lock) {
                foreach (var server in _discoveredServices) {
                    Console.WriteLine($"Found server: {server.Value.HostName}");
                }
                return _discoveredServices.Values.Where(s => s.HostName.Contains(serviceName,StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
        }

        /// <summary>
        /// Disposes resources used by the NetworkServiceDiscovery
        /// </summary>
        public void Dispose() {
            if (_isDisposed) return;
            Console.WriteLine("Disposing...");
            _sd.ServiceInstanceDiscovered -= OnServiceInstanceDiscovered;
            _mdns.Stop();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed() {
            if (!_isDisposed) return;
            throw new ObjectDisposedException(nameof(NetworkServiceDiscovery));
        }
    }
}