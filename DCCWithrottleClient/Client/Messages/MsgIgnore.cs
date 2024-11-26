namespace DCCWithrottleClient.Client.Messages;

public class MsgIgnore() : IClientMsg {
    public void Process(string commandStr) { }

    public override string ToString() {
        return "MSG:Ignore";
    }
    
    public  string ActionTaken { get; private set; } = string.Empty;

}