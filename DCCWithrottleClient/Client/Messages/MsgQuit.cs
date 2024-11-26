namespace DCCWithrottleClient.Client.Messages;

public class MsgQuit() : IClientMsg {
    public void Process(string commandStr) { }

    public override string ToString() {
        return "MSG:Quit";
    }
    
    public  string ActionTaken { get; private set; } = string.Empty;

}