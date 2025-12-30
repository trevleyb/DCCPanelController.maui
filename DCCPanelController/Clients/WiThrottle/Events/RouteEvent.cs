using DCCPanelController.Clients.WiThrottle.Client;
using DCCPanelController.Models.DataModel.Accessories;

namespace DCCPanelController.Clients.WiThrottle.Events;

public class RouteEvent(string systemName, string userName, RouteStateEnum stateEnum, AccessorySource? source = null) : EventArgs, IClientEvent {
    public RouteEvent(string systemName, char state, AccessorySource? source = null) : this(systemName, "", state.ToString(), source) { }
    public RouteEvent(string systemName, string state, AccessorySource? source = null) : this(systemName, "", state, source) { }

    public RouteEvent(string systemName, string userName, string state, AccessorySource? source = null) : this(systemName, userName, state switch {
        "1"        => RouteStateEnum.Unknown,
        "2" or "C" => RouteStateEnum.Active,
        "4" or "T" => RouteStateEnum.Inactive,
        _          => RouteStateEnum.Unknown
    }, source) { }

    public string SystemName { get; set; } = systemName;
    public string UserName { get; set; } = userName;
    public AccessorySource? Source { get; set; } = source;
    public RouteStateEnum StateEnum { get; set; } = stateEnum;

    public string State =>
        StateEnum switch {
            RouteStateEnum.Unknown  => "Unknown",
            RouteStateEnum.Active   => "Active",
            RouteStateEnum.Inactive => "Inactive",
            _                       => "Unknown"
        };

    public override string ToString() {
        return $"ROUTE: {SystemName}:{UserName} => {State}";
    }
}