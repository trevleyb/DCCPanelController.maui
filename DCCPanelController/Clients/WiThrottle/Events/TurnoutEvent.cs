using DCCPanelController.Clients.WiThrottle.Client;
using DCCPanelController.Models.DataModel.Accessories;

namespace DCCPanelController.Clients.WiThrottle.Events;

public class TurnoutEvent(string systemName, string userName, TurnoutStateEnum stateEnum) : EventArgs, IClientEvent {
    public TurnoutEvent(string systemName, char state, AccessorySource source = AccessorySource.WiThrottle) : this(systemName, "", state.ToString(), source) { }
    public TurnoutEvent(string systemName, string state, AccessorySource source = AccessorySource.WiThrottle) : this(systemName, "", state, source) { }

    public TurnoutEvent(string systemName, string userName, string state, AccessorySource source = AccessorySource.WiThrottle) : this(systemName, userName, state switch {
        "1"        => TurnoutStateEnum.Unknown,
        "2" or "C" => TurnoutStateEnum.Closed,
        "4" or "T" => TurnoutStateEnum.Thrown,
        _          => TurnoutStateEnum.Unknown
    }) { }

    public string SystemName { get; set; } = systemName;
    public string UserName { get; set; } = userName;
    public TurnoutStateEnum StateEnum { get; set; } = stateEnum;

    public string State =>
        StateEnum switch {
            TurnoutStateEnum.Unknown => "Unknown",
            TurnoutStateEnum.Closed  => "Closed",
            TurnoutStateEnum.Thrown  => "Thrown",
            _                        => "Unknown"
        };

    public override string ToString() {
        return $"TURNOUT: {SystemName}:{UserName} => {State}";
    }
}