namespace DCCPanelController.Clients.WiThrottle.Messages;

public class MsgIgnore(string commandStr) : ClientMsg, IClientMsg {
    public override string ToString() {
        return $"MSG:Ignore => {commandStr}";
    }
}