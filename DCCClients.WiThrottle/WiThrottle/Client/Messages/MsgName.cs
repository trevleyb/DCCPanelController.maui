using DCCClients.WiThrottle.Client.Events;

namespace DCCClients.WiThrottle.Client.Messages;

public class MsgName : ClientMsg, IClientMsg {
    private readonly string _commandStr;

    public MsgName(string commandStr) {
        _commandStr = commandStr;
        Add(new MessageEvent("SystemName", commandStr[1..]));
    }

    public override string ToString() {
        return $"MSG:SystemName => {_commandStr}";
    }
}