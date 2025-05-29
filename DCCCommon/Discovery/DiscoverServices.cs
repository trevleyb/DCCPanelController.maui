using DCCCommon.Common;

namespace DCCCommon.Discovery;

/// <summary>
///     Factory for creating service discovery instances
/// </summary>
public static class DiscoverServices {
    public static async Task<IResult<List<DiscoveredService>>> SearchForServicesByTypeAsync(string type, string subtype = "") {
        return type.ToLower() switch {
            "jmri"       => await SearchForJmriServicesAsync(),
            "withrottle" => await SearchForWiThrottleServicesAsync(),
            _            => await SearchForServicesAsync(type, subtype)
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