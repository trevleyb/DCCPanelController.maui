namespace DCCClients.WiThrottle.WiThrottle.Client.Commands;

public class RouteCommand(string systemName) : IClientCmd {
    public string SystemName { get; set; } = systemName;
    public string Command => $"PRA2{SystemName}";
}