namespace DCCWithrottleClient.Client.Events;

public class RouteEvent(string systemName, string userName, RouteStateEnum stateEnum) : EventArgs, IClientEvent {
    public RouteEvent(string systemName, char state) : this(systemName, "", state.ToString()) { }
    public RouteEvent(string systemName, string state) : this(systemName, "", state) { }

    public RouteEvent(string systemName, string userName, string state) : this(systemName, userName, state switch {
        "1"        => RouteStateEnum.Unknown,
        "2" or "C" => RouteStateEnum.Active,
        "4" or "T" => RouteStateEnum.Inactive,
        _          => RouteStateEnum.Unknown
    }) { }

    public string SystemName { get; set; } = systemName;
    public string UserName { get; set; } = userName;
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