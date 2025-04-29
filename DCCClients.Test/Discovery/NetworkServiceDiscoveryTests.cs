using System;
using System.Linq;
using System.Threading.Tasks;
using DCCClients.Discovery;
using NUnit.Framework;

namespace DCCClients.Test.Discovery
{
    [TestFixture]
    public class NetworkServiceDiscoveryTests
    {
        private INetworkServiceDiscovery _discovery;
        private JmriServiceDiscovery _jmriDiscovery;
        private WiThrottleServiceDiscovery _wiThrottleDiscovery;
        
        [SetUp]
        public void Setup()
        {
            _discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            _jmriDiscovery = new JmriServiceDiscovery(_discovery);
            _wiThrottleDiscovery = new WiThrottleServiceDiscovery(_discovery);
        }
        
        [TearDown]
        public void TearDown()
        {
            _discovery.Dispose();
        }
        
        [Test]
        [Category("Integration")]
        public async Task DiscoverGenericServices_ShouldFindServices()
        {
            // Arrange
            const string httpServiceType = "_http._tcp.local";
            const int timeoutSeconds = 5;
            
            // Act
            Console.WriteLine($"Discovering generic {httpServiceType} services...");
            var services = await _discovery.DiscoverServicesAsync(httpServiceType, timeoutSeconds);
            
            // Assert
            Console.WriteLine($"Found {services.Count} {httpServiceType} services:");
            foreach (var service in services)
            {
                Console.WriteLine($"- {service.InstanceName} at {service.GetUrl()}");
                Console.WriteLine($"  Host: {service.HostName}");
                Console.WriteLine($"  Port: {service.Port}");
                Console.WriteLine($"  Addresses: {string.Join(", ", service.Addresses)}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", service.TxtRecords)}");
                Console.WriteLine();
            }
            
            // We don't assert that services were found, as there might not be any on the network
            // This test is primarily for manual verification
            Assert.Pass($"Successfully completed discovery of {httpServiceType} services");
        }
        
        [Test]
        [Category("Integration")]
        public async Task DiscoverJmriServers_ShouldFindJmriServers()
        {
            // Arrange
            const int timeoutSeconds = 5;
            
            // Act
            Console.WriteLine("Discovering JMRI servers...");
            var servers = await _jmriDiscovery.DiscoverJmriServersAsync(timeoutSeconds);
            var urls = await _jmriDiscovery.DiscoverJmriServerUrlsAsync(timeoutSeconds);
            
            // Assert
            Console.WriteLine($"Found {servers.Count} JMRI servers:");
            foreach (var server in servers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                Console.WriteLine($"  Host: {server.HostName}");
                Console.WriteLine($"  Port: {server.Port}");
                Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                Console.WriteLine();
            }
            
            Console.WriteLine("JMRI Server URLs:");
            foreach (var url in urls)
            {
                Console.WriteLine($"- {url}");
            }
            
            // We don't assert that servers were found, as there might not be any on the network
            // This test is primarily for manual verification
            Assert.Pass("Successfully completed discovery of JMRI servers");
        }
        
        [Test]
        [Category("Integration")]
        public async Task DiscoverWiThrottleServers_ShouldFindWiThrottleServers()
        {
            // Arrange
            const int timeoutSeconds = 5;
            
            // Act
            Console.WriteLine("Discovering WiThrottle servers...");
            var servers = await _wiThrottleDiscovery.DiscoverWiThrottleServersAsync(timeoutSeconds);
            var addresses = await _wiThrottleDiscovery.DiscoverWiThrottleServerAddressesAsync(timeoutSeconds);
            
            // Assert
            Console.WriteLine($"Found {servers.Count} WiThrottle servers:");
            foreach (var server in servers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                Console.WriteLine($"  Host: {server.HostName}");
                Console.WriteLine($"  Port: {server.Port}");
                Console.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                Console.WriteLine();
            }
            
            Console.WriteLine("WiThrottle Server Addresses:");
            foreach (var (address, port) in addresses)
            {
                Console.WriteLine($"- {address}:{port}");
            }
            
            // We don't assert that servers were found, as there might not be any on the network
            // This test is primarily for manual verification
            Assert.Pass("Successfully completed discovery of WiThrottle servers");
        }
        
        [Test]
        [Category("Integration")]
        public async Task DiscoverAllServices_ShouldFindAllSupportedServices()
        {
            // Arrange
            const int timeoutSeconds = 5;
            
            // Act & Assert
            Console.WriteLine("=== DISCOVERING ALL SUPPORTED SERVICES ===");
            
            // Discover JMRI servers
            Console.WriteLine("\n=== JMRI SERVERS ===");
            var jmriServers = await _jmriDiscovery.DiscoverJmriServersAsync(timeoutSeconds);
            Console.WriteLine($"Found {jmriServers.Count} JMRI servers");
            foreach (var server in jmriServers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
            }
            
            // Discover WiThrottle servers
            Console.WriteLine("\n=== WITHROTTLE SERVERS ===");
            var wiThrottleServers = await _wiThrottleDiscovery.DiscoverWiThrottleServersAsync(timeoutSeconds);
            Console.WriteLine($"Found {wiThrottleServers.Count} WiThrottle servers");
            foreach (var server in wiThrottleServers)
            {
                Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
            }
            
            // Discover all HTTP services for comparison
            Console.WriteLine("\n=== ALL HTTP SERVICES ===");
            var httpServices = await _discovery.DiscoverServicesAsync("_http._tcp.local", timeoutSeconds);
            Console.WriteLine($"Found {httpServices.Count} HTTP services");
            foreach (var service in httpServices)
            {
                Console.WriteLine($"- {service.InstanceName} at {service.GetUrl()}");
                Console.WriteLine($"  TXT Records: {string.Join(", ", service.TxtRecords)}");
            }
            
            // We don't assert that servers were found, as there might not be any on the network
            // This test is primarily for manual verification
            Assert.Pass("Successfully completed discovery of all supported services");
        }
        
        [Test]
        [Category("Integration")]
        [Explicit("This test runs continuously and requires manual termination")]
        public async Task ContinuousDiscovery_ShouldContinuouslyMonitorServices()
        {
            // This test continuously monitors for services until manually stopped
            // It's useful for testing in a dynamic environment where services come and go
            
            Console.WriteLine("Starting continuous service discovery...");
            Console.WriteLine("Press Ctrl+C to stop the test");
            
            while (true)
            {
                Console.WriteLine("\n=== JMRI SERVERS ===");
                var jmriServers = await _jmriDiscovery.DiscoverJmriServersAsync(2);
                Console.WriteLine($"Found {jmriServers.Count} JMRI servers");
                foreach (var server in jmriServers)
                {
                    Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                }
                
                Console.WriteLine("\n=== WITHROTTLE SERVERS ===");
                var wiThrottleServers = await _wiThrottleDiscovery.DiscoverWiThrottleServersAsync(2);
                Console.WriteLine($"Found {wiThrottleServers.Count} WiThrottle servers");
                foreach (var server in wiThrottleServers)
                {
                    Console.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                }
                
                // Wait before next scan
                await Task.Delay(5000);
            }
        }
    }
}
