namespace DccClients.WiThrottle.Client.Messages;

public class MsgHeartbeat : ClientMsg, IClientMsg {
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

    public int HeartbeatSeconds { get; private set; }

    public override string ToString() {
        return $"MSG:Heartbeat => {_commandStr}";
    }
}