using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Discovers WiThrottle servers on the local network
    /// </summary>
    public class WiThrottleServiceDiscovery : IServiceDiscovery
    {
        private readonly INetworkServiceDiscovery _discovery;
        
        /// <summary>
        /// Creates a new instance of the WiThrottleServiceDiscovery class
        /// </summary>
        /// <param name="discovery">The network service discovery implementation to use</param>
        public WiThrottleServiceDiscovery(INetworkServiceDiscovery discovery)
        {
            _discovery = discovery;
        }
        
        /// <summary>
        /// Discovers WiThrottle servers on the local network
        /// </summary>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered WiThrottle servers</returns>
        public async Task<List<DiscoveredService>> DiscoverServersAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default)
        {
            // WiThrottle servers advertise as _withrottle._tcp.local
            return await _discovery.DiscoverServicesAsync("withrottle", timeoutSeconds, cancellationToken);
        }

        /// <summary>
        /// Gets the address and port pairs for discovered WiThrottle servers
        /// </summary>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of (address, port) tuples for WiThrottle servers</returns>
        public async Task<List<string>> DiscoverServerUrlsAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default) { 
            var servers = await DiscoverServersAsync(timeoutSeconds, cancellationToken);
           
            return servers.Select(s => 
            {
                // Get the preferred address (IPv4 if available)
                string address = s.HostName;
                if (s.Addresses.Count > 0)
                {
                    var ipv4 = s.Addresses.Find(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    address = (ipv4 ?? s.Addresses[0]).ToString();
                }
                else if (address.EndsWith("."))
                {
                    address = address.Substring(0, address.Length - 1);
                }
                
                return $"{address}:{s.Port}";
            }).ToList();
        }
    }
}
