using System;
using System.Threading.Tasks;
using DCCClients.Discovery;
using NUnit.Framework;

namespace DCCClients.Test.Discovery
{
    /// <summary>
    /// A simple console-based test for service discovery.
    /// This test is designed to be run manually to verify service discovery functionality.
    /// </summary>
    [TestFixture]
    [Category("Manual")]
    [Explicit("This test is for manual execution only")]
    public class ServiceDiscoveryConsoleTest
    {
        [Test]
        public async Task RunConsoleTest()
        {
            Console.WriteLine("=== Network Service Discovery Console Test ===");
            Console.WriteLine("This test will search for JMRI and WiThrottle servers on your network.");
            
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            // Search for JMRI servers
            Console.WriteLine("\nSearching for JMRI servers...");
            var jmriServers = await jmriDiscovery.DiscoverServersAsync(10);
            
            if (jmriServers.Count == 0) {
                Console.WriteLine("No JMRI servers found.");
            } else {
                Console.WriteLine($"Found {jmriServers.Count} JMRI servers:");
                foreach (var server in jmriServers) {
                    Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                    Console.WriteLine($"  Host: {server.HostName}");
                    Console.WriteLine($"  Port: {server.Port}");
                    Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                    Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                }
            }
            
            // Search for WiThrottle servers
            Console.WriteLine("\nSearching for WiThrottle servers...");
            var wiThrottleServers = await wiThrottleDiscovery.DiscoverServersAsync(10);
            
            if (wiThrottleServers.Count == 0) {
                Console.WriteLine("No WiThrottle servers found.");
            } else {
                Console.WriteLine($"Found {wiThrottleServers.Count} WiThrottle servers:");
                foreach (var server in wiThrottleServers) {
                    Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                    Console.WriteLine($"  Host: {server.HostName}");
                    Console.WriteLine($"  Port: {server.Port}");
                    Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                    Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                }
                
                var addresses = await wiThrottleDiscovery.DiscoverServerUrlsAsync(1);
                Console.WriteLine("\nWiThrottle Server Addresses:");
                foreach (var url in addresses) {
                    Console.WriteLine($"- {url}");
                }
            }
            Console.WriteLine("\nFinished.");
        }
        
        [Test, Ignore("Skipping")]
        public async Task RunContinuousDiscoveryTest()
        {
            Console.WriteLine("=== Continuous Network Service Discovery Test ===");
            Console.WriteLine("This test will continuously search for JMRI and WiThrottle servers on your network.");
            Console.WriteLine("Press Ctrl+C to exit.");
            
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== Service Discovery Scan at {DateTime.Now} ===");
                
                // Search for JMRI servers
                Console.WriteLine("\n=== JMRI SERVERS ===");
                var jmriServers = await jmriDiscovery.DiscoverServersAsync(3);
                
                if (jmriServers.Count == 0)
                {
                    Console.WriteLine("No JMRI servers found.");
                }
                else
                {
                    Console.WriteLine($"Found {jmriServers.Count} JMRI servers:");
                    foreach (var server in jmriServers)
                    {
                        Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                    }
                }
                
                // Search for WiThrottle servers
                Console.WriteLine("\n=== WITHROTTLE SERVERS ===");
                var wiThrottleServers = await wiThrottleDiscovery.DiscoverServersAsync(3);
                
                if (wiThrottleServers.Count == 0)
                {
                    Console.WriteLine("No WiThrottle servers found.");
                }
                else
                {
                    Console.WriteLine($"Found {wiThrottleServers.Count} WiThrottle servers:");
                    foreach (var server in wiThrottleServers)
                    {
                        Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                    }
                }
                
                Console.WriteLine("\nWaiting 5 seconds before next scan...");
                await Task.Delay(5000);
            }
        }
    }
}
