namespace DCCClients.Discovery
{
    /// <summary>
    /// Factory for creating service discovery instances
    /// </summary>
    public static class ServiceDiscoveryFactory
    {

        public static IServiceDiscovery DiscoverServices(string type) {
            return type switch {
                "jmri"       => CreateJmriServiceDiscovery(),
                "withrottle" => CreateWiThrottleServiceDiscovery(),
                _            => throw new ArgumentException("Invalid service type", nameof(type))
            };
        }
        
        /// <summary>
        /// Creates a new JMRI service discovery
        /// </summary>
        /// <returns>A JMRI service discovery instance</returns>
        public static JmriServiceDiscovery CreateJmriServiceDiscovery() {
            return new JmriServiceDiscovery(new NetworkServiceDiscovery());
        }
        
        /// <summary>
        /// Creates a new WiThrottle service discovery
        /// </summary>
        /// <returns>A WiThrottle service discovery instance</returns>
        public static WiThrottleServiceDiscovery CreateWiThrottleServiceDiscovery() {
            return new WiThrottleServiceDiscovery(new NetworkServiceDiscovery());
        }
        
        /// <summary>
        /// Creates a new generic network service discovery
        /// </summary>
        /// <returns>A network service discovery instance</returns>
        public static INetworkServiceDiscovery CreateNetworkServiceDiscovery() {
            return new NetworkServiceDiscovery();
        }
    }
}
