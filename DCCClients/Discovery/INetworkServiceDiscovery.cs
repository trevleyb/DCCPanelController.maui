using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Interface for network service discovery
    /// </summary>
    public interface INetworkServiceDiscovery : IDisposable
    {
        /// <summary>
        /// Discovers services of the specified type on the local network
        /// </summary>
        /// <param name="serviceType">The service type to discover (e.g., "_http._tcp.local")</param>
        /// <param name="timeoutSeconds">How long to search for services in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of discovered service information</returns>
        Task<List<DiscoveredService>> DiscoverServicesAsync(string serviceType, int timeoutSeconds = 5, CancellationToken cancellationToken = default);
    }
}
