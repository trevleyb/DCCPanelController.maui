namespace DCCWithrottleClient.Client.Messages;

public class MsgQuit(string commandStr) : ClientMsg, IClientMsg {
    public override string ToString() {
        return $"MSG:Quit => {commandStr}";
    }
}