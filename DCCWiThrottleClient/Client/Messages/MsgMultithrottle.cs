namespace DCCWiThrottleClient.Client.Messages;

public class MsgMultiThrottle() : IClientMsg {
    public void   Process(string commandStr) { }
    public string ActionTaken                { get; private set; } = string.Empty;

}