namespace DCCWithrottleClient.Client.Events;

public class TurnoutEvent(string systemName, string userName, TurnoutStateEnum stateEnum) : EventArgs, IClientEvent {
      public TurnoutEvent(string systemName, char state) : this(systemName, "", state.ToString()) { }
      public TurnoutEvent(string systemName, string state) : this(systemName, "", state) { }
      public TurnoutEvent(string systemName, string userName, string state) : this(systemName, userName, state switch {
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

    public override string ToString() => $"TURNOUT: {SystemName}:{UserName} => {State}";

}
