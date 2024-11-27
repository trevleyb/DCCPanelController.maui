using DCCWithrottleClient.Client.Events;

namespace DCCWithrottleClient.Client.Messages;

public class MsgName :ClientMsg, IClientMsg {
    private readonly string _commandStr;
    public MsgName(string commandStr) {
        _commandStr = commandStr;
        Add(new MessageEvent("SystemName", commandStr[1..]));
    }
    public override string ToString() {
        return $"MSG:SystemName => {_commandStr}";
    }
    
}