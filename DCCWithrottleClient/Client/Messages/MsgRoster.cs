namespace DCCWithrottleClient.Client.Messages;

public class MsgRoster() : IClientMsg {
    public void Process(string commandStr) { }

    public  string ActionTaken { get; private set; } = string.Empty;

    public override string ToString() {
        return "MSG:Roster";
    }
}