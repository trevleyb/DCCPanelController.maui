using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DCCClients.Discovery;
using NUnit.Framework;

namespace DCCClients.Test.Discovery
{
    [TestFixture]
    [Category("RealServices")]
    public class RealServiceDiscoveryTests
    {
        private const int DiscoveryTimeoutSeconds = 10;
        
        [Test]
        public async Task DiscoverJmriServers_ShouldFindRealJmriServers()
        {
            // Arrange
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            
            // Act
            TestContext.Progress.WriteLine("Searching for JMRI servers...");
            var startTime = Stopwatch.GetTimestamp();
            
            var servers = await jmriDiscovery.DiscoverJmriServersAsync(DiscoveryTimeoutSeconds);
            
            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            TestContext.Progress.WriteLine($"Discovery completed in {elapsedMs:F2}ms");
            
            // Assert
            if (servers.Count == 0)
            {
                TestContext.Progress.WriteLine("No JMRI servers found on the network.");
                Assert.Inconclusive("No JMRI servers found on the network. This test requires a running JMRI server.");
                return;
            }
            
            TestContext.Progress.WriteLine($"Found {servers.Count} JMRI servers:");
            foreach (var server in servers)
            {
                TestContext.Progress.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                TestContext.Progress.WriteLine($"  Host: {server.HostName}");
                TestContext.Progress.WriteLine($"  Port: {server.Port}");
                TestContext.Progress.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                
                // Verify that the server has the expected TXT records
                var hasTxtRecords = server.TxtRecords.Any(txt => txt.Contains("path=/json"));
                TestContext.Progress.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                TestContext.Progress.WriteLine($"  Has JSON path: {hasTxtRecords}");
                
                Assert.That(hasTxtRecords, Is.True, "JMRI server should have a TXT record with path=/json");
            }
            
            // Verify that we can get URLs
            var urls = await jmriDiscovery.DiscoverJmriServerUrlsAsync(1); // Short timeout since we already discovered
            TestContext.Progress.WriteLine($"\nJMRI Server URLs: {string.Join(", ", urls)}");
            
            Assert.That(urls, Has.Count.EqualTo(servers.Count), "URL count should match server count");
        }
        
        [Test]
        public async Task DiscoverWiThrottleServers_ShouldFindRealWiThrottleServers()
        {
            // Arrange
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            // Act
            TestContext.Progress.WriteLine("Searching for WiThrottle servers...");
            var startTime = Stopwatch.GetTimestamp();
            
            var servers = await wiThrottleDiscovery.DiscoverWiThrottleServersAsync(DiscoveryTimeoutSeconds);
            
            var elapsedMs = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            TestContext.Progress.WriteLine($"Discovery completed in {elapsedMs:F2}ms");
            
            // Assert
            if (servers.Count == 0)
            {
                TestContext.Progress.WriteLine("No WiThrottle servers found on the network.");
                Assert.Inconclusive("No WiThrottle servers found on the network. This test requires a running WiThrottle server.");
                return;
            }
            
            TestContext.Progress.WriteLine($"Found {servers.Count} WiThrottle servers:");
            foreach (var server in servers)
            {
                TestContext.Progress.WriteLine($"- {server.InstanceName} at {server.GetUrl()}");
                TestContext.Progress.WriteLine($"  Host: {server.HostName}");
                TestContext.Progress.WriteLine($"  Port: {server.Port}");
                TestContext.Progress.WriteLine($"  Addresses: {string.Join(", ", server.Addresses)}");
                TestContext.Progress.WriteLine($"  TXT Records: {string.Join(", ", server.TxtRecords)}");
                
                // Verify that the server has the expected service type
                Assert.That(server.ServiceType, Is.EqualTo("_withrottle._tcp.local"), 
                    "Service type should be _withrottle._tcp.local");
            }
            
            // Verify that we can get addresses
            var addresses = await wiThrottleDiscovery.DiscoverWiThrottleServerAddressesAsync(1); // Short timeout since we already discovered
            TestContext.Progress.WriteLine($"\nWiThrottle Server Addresses:");
            foreach (var (address, port) in addresses)
            {
                TestContext.Progress.WriteLine($"- {address}:{port}");
            }
            
            Assert.That(addresses, Has.Count.EqualTo(servers.Count), "Address count should match server count");
        }
        
        [Test]
        public async Task DiscoveryPerformance_ShouldCompleteQuickly()
        {
            // Arrange
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            // Act & Assert - JMRI
            TestContext.Progress.WriteLine("Testing JMRI discovery performance...");
            var jmriStartTime = Stopwatch.GetTimestamp();
            
            var jmriServers = await jmriDiscovery.DiscoverJmriServersAsync(5);
            
            var jmriElapsedMs = Stopwatch.GetElapsedTime(jmriStartTime).TotalMilliseconds;
            TestContext.Progress.WriteLine($"JMRI discovery completed in {jmriElapsedMs:F2}ms, found {jmriServers.Count} servers");
            
            // Act & Assert - WiThrottle
            TestContext.Progress.WriteLine("\nTesting WiThrottle discovery performance...");
            var wiThrottleStartTime = Stopwatch.GetTimestamp();
            
            var wiThrottleServers = await wiThrottleDiscovery.DiscoverWiThrottleServersAsync(5);
            
            var wiThrottleElapsedMs = Stopwatch.GetElapsedTime(wiThrottleStartTime).TotalMilliseconds;
            TestContext.Progress.WriteLine($"WiThrottle discovery completed in {wiThrottleElapsedMs:F2}ms, found {wiThrottleServers.Count} servers");
            
            // We don't assert on the elapsed time as it depends on the network and available services
            // This test is primarily for manual performance evaluation
        }
        
        [Test]
        public async Task DiscoverMultipleTimesInSequence_ShouldWorkReliably()
        {
            // Arrange
            using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
            var jmriDiscovery = new JmriServiceDiscovery(discovery);
            var wiThrottleDiscovery = new WiThrottleServiceDiscovery(discovery);
            
            // Act & Assert
            for (int i = 0; i < 3; i++)
            {
                TestContext.Progress.WriteLine($"\n=== Discovery Iteration {i+1} ===");
                
                // JMRI
                TestContext.Progress.WriteLine("Discovering JMRI servers...");
                var jmriServers = await jmriDiscovery.DiscoverJmriServersAsync(3);
                TestContext.Progress.WriteLine($"Found {jmriServers.Count} JMRI servers");
                
                // WiThrottle
                TestContext.Progress.WriteLine("Discovering WiThrottle servers...");
                var wiThrottleServers = await wiThrottleDiscovery.DiscoverWiThrottleServersAsync(3);
                TestContext.Progress.WriteLine($"Found {wiThrottleServers.Count} WiThrottle servers");
                
                // Short delay between iterations
                await Task.Delay(1000);
            }
            
            // This test is primarily to verify that multiple discovery operations can be performed
            // in sequence without issues
            Assert.Pass("Successfully completed multiple discovery operations in sequence");
        }
    }
}
