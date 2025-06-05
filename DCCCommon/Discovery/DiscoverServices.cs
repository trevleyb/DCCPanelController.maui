using DCCCommon.Client;
using DCCCommon.Common;

namespace DCCCommon.Discovery;

/// <summary>
///     Factory for creating service discovery instances
/// </summary>
public static class DiscoverServices {
    public static async Task<IResult<List<DiscoveredService>>> SearchForServicesByTypeAsync(DccClientType type, string subtype = "") {
        return type switch {
            DccClientType.Jmri       => await SearchForJmriServicesAsync(),
            DccClientType.WiThrottle => await SearchForWiThrottleServicesAsync(),
            _                        => throw new InvalidOperationException("Invalid Service type to search for.")
        };
    }

    public static async Task<IResult<List<DiscoveredService>>> SearchForJmriServicesAsync() {
        return await SearchForServicesAsync("_http._tcp", "jmri");
    }

    public static async Task<IResult<List<DiscoveredService>>> SearchForWiThrottleServicesAsync() {
        return await SearchForServicesAsync("_withrottle._tcp");
    }

    private static async Task<IResult<List<DiscoveredService>>> SearchForServicesAsync(string serviceType, string subtype = "") {
        using (INetworkServiceDiscovery discoveryService = new MdnsNetworkServiceDiscovery()) {
            try {
                var timeout = TimeSpan.FromSeconds(5);
                return await discoveryService.DiscoverServicesAsync(serviceType, subtype, timeout);
            } catch (Exception ex) {
                return Result<List<DiscoveredService>>.Fail("Error Occurred", ex);
            }
        }
    }
}