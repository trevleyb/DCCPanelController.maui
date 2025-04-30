using System;
using System.Threading.Tasks;

namespace DCCClients.Discovery
{
    /// <summary>
    /// Example usage of the service discovery classes
    /// </summary>
    public static class ServiceDiscoveryExample
    {
        /// <summary>
        /// Demonstrates how to discover JMRI servers
        /// </summary>
        public static async Task DiscoverJmriServersExample()
        {
            // Create a JMRI service discovery
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            
            Console.WriteLine("Searching for JMRI servers...");
            
            // Discover JMRI servers
            var servers = await jmriDiscovery.DiscoverServersAsync();
            
            if (servers.Count == 0)
            {
                Console.WriteLine("No JMRI servers found.");
                return;
            }
            
            Console.WriteLine($"Found {servers.Count} JMRI servers:");
            foreach (var server in servers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                Console.WriteLine($"  Host: {server.HostName}");
                Console.WriteLine($"  Port: {server.Port}");
                Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
            }
        }
        
        /// <summary>
        /// Demonstrates how to discover WiThrottle servers
        /// </summary>
        public static async Task DiscoverWiThrottleServersExample()
        {
            // Create a WiThrottle service discovery
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            Console.WriteLine("Searching for WiThrottle servers...");
            
            // Discover WiThrottle servers
            var servers = await wiThrottleDiscovery.DiscoverServersAsync();
            
            if (servers.Count == 0)
            {
                Console.WriteLine("No WiThrottle servers found.");
                return;
            }
            
            Console.WriteLine($"Found {servers.Count} WiThrottle servers:");
            foreach (var server in servers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                Console.WriteLine($"  Host: {server.HostName}");
                Console.WriteLine($"  Port: {server.Port}");
                Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
            }
            
            // Get address and port pairs
            var addresses = await wiThrottleDiscovery.DiscoverServerUrlsAsync();
            Console.WriteLine("\nWiThrottle server addresses:");
            foreach (var url in addresses)
            {
                Console.WriteLine($"- {url}");
            }
        }
    }
}
