# Network Service Discovery

This module provides classes for discovering network services using Multicast DNS (mDNS), specifically targeting JMRI and WiThrottle servers on iOS and MacCatalyst.

## Requirements

- .NET 8.0 or later
- Makaretu.Dns.Multicast.New NuGet package

## iOS and MacCatalyst Configuration

To use mDNS service discovery on iOS and MacCatalyst, you need to add the following to your Info.plist:

```xml
<key>NSLocalNetworkUsageDescription</key>
<string>This app needs to discover JMRI and WiThrottle servers on your local network</string>
<key>NSBonjourServices</key>
<array>
    <string>_http._tcp</string>
    <string>_withrottle._tcp</string>
</array>
```

## Usage

### Discovering JMRI Servers

```csharp
// Create a JMRI service discovery
var jmriDiscovery = ServiceDiscoveryFactory.CreateJmriServiceDiscovery();

// Discover JMRI servers
var servers = await jmriDiscovery.DiscoverJmriServersAsync();

// Get JMRI server URLs
var urls = await jmriDiscovery.DiscoverJmriServerUrlsAsync();
foreach (var url in urls)
{
    Console.WriteLine($"Found JMRI server at: {url}");
}
```

### Discovering WiThrottle Servers

```csharp
// Create a WiThrottle service discovery
var wiThrottleDiscovery = ServiceDiscoveryFactory.CreateWiThrottleServiceDiscovery();

// Discover WiThrottle servers
var servers = await wiThrottleDiscovery.DiscoverWiThrottleServersAsync();

// Get WiThrottle server addresses and ports
var addresses = await wiThrottleDiscovery.DiscoverWiThrottleServerAddressesAsync();
foreach (var (address, port) in addresses)
{
    Console.WriteLine($"Found WiThrottle server at: {address}:{port}");
}
```

### Discovering Other Services

You can discover any type of service by using the generic `NetworkServiceDiscovery` class:

```csharp
// Create a generic network service discovery
using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();

// Discover services of a specific type
var services = await discovery.DiscoverServicesAsync("_myservice._tcp.local");

foreach (var service in services)
{
    Console.WriteLine($"Found service: {service.InstanceName} at {service.GetUrl()}");
}
```

## Extending for Other Services

To add support for discovering other types of services, you can create a new class following the pattern of `JmriServiceDiscovery` or `WiThrottleServiceDiscovery`:

```csharp
public class MyServiceDiscovery
{
    private readonly INetworkServiceDiscovery _discovery;
    
    public MyServiceDiscovery(INetworkServiceDiscovery discovery)
    {
        _discovery = discovery;
    }
    
    public async Task<List<DiscoveredService>> DiscoverMyServicesAsync(int timeoutSeconds = 5, CancellationToken cancellationToken = default)
    {
        // Replace with your service type
        var services = await _discovery.DiscoverServicesAsync("_myservice._tcp.local", timeoutSeconds, cancellationToken);
        
        // Apply any filtering if needed
        return services.Where(s => /* your condition */).ToList();
    }
}
```

Then add a factory method to `ServiceDiscoveryFactory`:

```csharp
public static MyServiceDiscovery CreateMyServiceDiscovery()
{
    return new MyServiceDiscovery(new NetworkServiceDiscovery());
}
```

## Resource Management

The `NetworkServiceDiscovery` class implements `IDisposable`, so make sure to dispose of it when you're done:

```csharp
using (var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery())
{
    // Use discovery
}
```

Or if you're using the specialized discovery classes:

```csharp
using var discovery = ServiceDiscoveryFactory.CreateNetworkServiceDiscovery();
var jmriDiscovery = new JmriServiceDiscovery(discovery);

// Use jmriDiscovery

// Make sure to dispose the underlying discovery when done
discovery.Dispose();
```
