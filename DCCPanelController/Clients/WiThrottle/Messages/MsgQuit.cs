namespace DCCPanelController.Clients.WiThrottle.Messages;

public class MsgQuit(string commandStr) : ClientMsg, IClientMsg {
    public override string ToString() {
        return $"MSG:Quit => {commandStr}";
    }
}