namespace DCCWiThrottleClient.Client.Messages;

public class MsgHeartbeat() : IClientMsg {
    public int HeartbeatSeconds { get; private set; }

    public void Process(string commandStr) {
        if (commandStr.Length == 0) return;
        if (commandStr[0] == '*' || commandStr[0] == '#') {
            if (int.TryParse(commandStr.Substring(1), out var secs)) {
                HeartbeatSeconds = secs;
            }
        }
    }

    public  string ActionTaken { get; private set; } = string.Empty;

    public override string ToString() {
        return "MSG:Heartbeat";
    }
}