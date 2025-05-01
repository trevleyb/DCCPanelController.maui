using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Discovers JMRI servers on the local network
    /// </summary>
    public class JmriServiceDiscovery : IServiceDiscovery
    {
        private readonly INetworkServiceDiscovery _discovery;
        
        /// <summary>
        /// Creates a new instance of the JmriServiceDiscovery class
        /// </summary>
        /// <param name="discovery">The network service discovery implementation to use</param>
        public JmriServiceDiscovery(INetworkServiceDiscovery discovery) {
            _discovery = discovery;
        }
        
        /// <summary>
        /// Discovers JMRI servers on the local network
        /// </summary>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered JMRI servers</returns>
        public async Task<List<DiscoveredService>> DiscoverServersAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default)
        {
            // JMRI servers advertise as _http._tcp.local
            var services = await _discovery.DiscoverServicesAsync("jmri", timeoutSeconds, cancellationToken);
            return services
                .Where(s => s.TxtRecords.Any(txt => txt.Contains("path=/json")))
                .ToList();
        }
        
        /// <summary>
        /// Gets the URLs for discovered JMRI servers
        /// </summary>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of JMRI server URLs</returns>
        public async Task<List<string>> DiscoverServerUrlsAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default) {
            var servers = await DiscoverServersAsync(timeoutSeconds, cancellationToken);
            return servers.Select(s => s.GetUrl()).ToList();
        }
    }
}
