namespace DCCClients.WiThrottle.Client.Messages;

public class MsgIgnore(string commandStr) : ClientMsg, IClientMsg {
    public override string ToString() {
        return $"MSG:Ignore => {commandStr}";
    }
}