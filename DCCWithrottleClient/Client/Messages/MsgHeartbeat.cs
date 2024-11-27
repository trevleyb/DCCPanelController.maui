namespace DCCWithrottleClient.Client.Messages;

public class MsgHeartbeat : ClientMsg, IClientMsg {
    public int HeartbeatSeconds { get; private set; }
    private readonly string _commandStr;

    public MsgHeartbeat(string commandStr) {
        _commandStr = commandStr;
        if (commandStr.Length == 0) return;
        if (commandStr[0] == '*' || commandStr[0] == '#') {
            if (int.TryParse(commandStr[1..], out var secs)) {
                HeartbeatSeconds = secs;
            }
        }
    }
    
    public override string ToString() {
        return $"MSG:Heartbeat => {_commandStr}";
    }
}