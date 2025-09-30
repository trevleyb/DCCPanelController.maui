using DCCPanelController.Helpers;

namespace DCCPanelController.Clients.Discovery;

/// <summary>
///     Factory for creating service discovery instances
/// </summary>
public static class DiscoverServices {

    public static async Task<IResult<List<DiscoveredService>>> SearchForJmriServicesAsync() {
        return await SearchForServicesAsync("_http._tcp", "jmri");
    }

    public static async Task<IResult<List<DiscoveredService>>> SearchForWiThrottleServicesAsync() {
        return await SearchForServicesAsync("_withrottle._tcp");
    }

    public static async Task<IResult<List<DiscoveredService>>> SearchForServicesAsync(string serviceType, string subtype = "") {
        using (INetworkServiceDiscovery discoveryService = new MdnsNetworkServiceDiscovery()) {
            try {
                var timeout = TimeSpan.FromSeconds(5);
                var results = await discoveryService.DiscoverServicesAsync(serviceType, subtype, timeout);
                return results;
            } catch (Exception ex) {
                return Result<List<DiscoveredService>>.Error(ex);
            }
        }
    }
}