using DCCCommon.Common;
using DCCCommon.Discovery;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Clients;

public static class DccClientFactory {
    public static IDccClient CreateClient(Profile profile, IDccClientSettings settings) {
        return settings switch {
            JmriSettings       => new JmriProxy(profile, settings),
            WiThrottleSettings => new WiThrottleProxy(profile, settings),
            SimulatorSettings  => new SimulatorProxy(profile, settings),
            _                  => throw new ArgumentOutOfRangeException($"Unknown Settings type {settings.GetType()}")
        };
    }

    public static async Task<IResult<List<DiscoveredService>>> FindServices(DccClientType clientType, string subType = "") {
        return clientType switch {
            DccClientType.Jmri       => await DiscoverServices.SearchForJmriServicesAsync(),
            DccClientType.WiThrottle => await DiscoverServices.SearchForWiThrottleServicesAsync(),
            DccClientType.Simulator  => await DiscoverServices.SearchForServicesAsync(subType),
            _                        => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null)
        };
    }
}