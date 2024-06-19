namespace DCCWiThrottleClient.Client.Messages;

public interface IClientMsg {
    void        Process(string commandStr);
    string ActionTaken { get; }
}