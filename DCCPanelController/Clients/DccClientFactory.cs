using DCCPanelController.Clients.Discovery;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Clients;

public static class DccClientFactory {
    public static IDccClient CreateClient(Profile profile, IDccClientSettings settings) => settings switch {
        JmriSettings       => new JmriDccClient(profile, settings),
        WiThrottleSettings => new WiThrottleDccClient(profile, settings),
        SimulatorSettings  => new SimulatorDccClient(profile, settings),
        _                  => throw new ArgumentOutOfRangeException($"Unknown Settings type {settings.GetType()}"),
    };
    
    public static async Task<IResult<List<DiscoveredService>>> FindServices(DccClientType clientType, string subType = "") => clientType switch {
        DccClientType.Jmri       => await DiscoverServices.SearchForJmriServicesAsync(),
        DccClientType.WiThrottle => await DiscoverServices.SearchForWiThrottleServicesAsync(),
        DccClientType.Simulator  => await DiscoverServices.SearchForServicesAsync(subType),
        _                        => throw new ArgumentOutOfRangeException(nameof(clientType), clientType, null),
    };

}
