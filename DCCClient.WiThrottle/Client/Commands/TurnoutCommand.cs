namespace DccClients.WiThrottle.Client.Commands;

public class TurnoutCommand(string systemName, TurnoutStateEnum state) : IClientCmd {
    public string SystemName { get; set; } = systemName;
    public TurnoutStateEnum State { get; set; } = state;

    private char CmdState =>
        State switch {
            TurnoutStateEnum.Thrown => 'T',
            TurnoutStateEnum.Closed => 'C',
            _                       => '2'
        };

    public string Command => $"PTA{CmdState}{SystemName}";
}