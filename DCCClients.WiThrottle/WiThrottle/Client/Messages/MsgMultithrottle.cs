namespace DCCClients.WiThrottle.Client.Messages;

public class MsgMultiThrottle : ClientMsg, IClientMsg {
    private readonly string _commandStr;

    public MsgMultiThrottle(string commandStr) {
        _commandStr = commandStr;
    }

    public override string ToString() {
        return $"MSG:Multithrottle => {_commandStr}";
    }
}