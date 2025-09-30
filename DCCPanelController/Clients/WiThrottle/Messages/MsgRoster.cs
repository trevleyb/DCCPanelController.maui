namespace DCCPanelController.Clients.WiThrottle.Messages;

public class MsgRoster : ClientMsg, IClientMsg {
    private readonly string _commandStr;

    public MsgRoster(string commandStr) {
        _commandStr = commandStr;
    }

    public override string ToString() {
        return $"MSG:Roster => {_commandStr}";
    }
}