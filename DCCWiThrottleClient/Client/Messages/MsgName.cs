namespace DCCWiThrottleClient.Client.Messages;

public class MsgName() : IClientMsg {
    public void Process(string commandStr) { }

    public override string ToString() {
        return "MSG:Name";
    }    
    public  string ActionTaken { get; private set; } = string.Empty;

}