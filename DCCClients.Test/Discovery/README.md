# Network Service Discovery Tests

This directory contains tests for the network service discovery functionality, specifically for discovering JMRI and WiThrottle servers on the local network.

## Test Categories

### Integration Tests

These tests attempt to discover actual services on the network:

- `NetworkServiceDiscoveryTests`: Tests the basic discovery functionality
- `RealServiceDiscoveryTests`: Tests discovery against real services with more detailed assertions

### Manual Tests

These tests are designed to be run manually for interactive testing:

- `ServiceDiscoveryConsoleTest`: A console-based test for manual verification

## Running the Tests

### Prerequisites

To run these tests successfully, you should have:

1. A running JMRI server with the JSON server enabled (for JMRI tests)
2. A running WiThrottle server (for WiThrottle tests)
3. Proper network connectivity between your test machine and the servers

### Running from Visual Studio

1. Open the Test Explorer
2. Filter by Category to find the tests you want to run:
   - "Integration" for the integration tests
   - "RealServices" for tests that require real services
   - "Manual" for the manual console tests
3. Right-click on a test and select "Run" or "Debug"

### Running from Command Line

```bash
# Run all tests
dotnet test DCCClients.Test

# Run integration tests
dotnet test DCCClients.Test --filter Category=Integration

# Run real service tests
dotnet test DCCClients.Test --filter Category=RealServices

# Run a specific test
dotnet test DCCClients.Test --filter FullyQualifiedName=DCCClients.Test.Discovery.NetworkServiceDiscoveryTests.DiscoverJmriServers_ShouldFindJmriServers
```

## Test Behavior

- The integration tests will output discovered services to the console
- If no services are found, the tests will not fail but will output a message indicating no services were found
- The `RealServiceDiscoveryTests` will mark tests as "Inconclusive" if no services are found
- The manual console tests require user interaction and will not automatically complete

## Troubleshooting

If the tests are not finding services:

1. Ensure your JMRI/WiThrottle servers are running
2. Check network connectivity (firewalls, VPNs, etc.)
3. Verify that multicast DNS (mDNS) is allowed on your network
4. Try increasing the discovery timeout in the tests
5. Run the manual console test for more detailed output

## iOS and MacCatalyst Considerations

When running these tests on iOS or MacCatalyst, ensure that:

1. The app has the necessary entitlements in Info.plist:
   ```xml
   <key>NSLocalNetworkUsageDescription</key>
   <string>This app needs to discover JMRI and WiThrottle servers on your local network</string>
   <key>NSBonjourServices</key>
   <array>
       <string>_http._tcp</string>
       <string>_withrottle._tcp</string>
   </array>
   ```

2. The app has been granted permission to access the local network
