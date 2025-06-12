using DccClients.WiThrottle.Client.Events;

namespace DccClients.WiThrottle.Client.Messages;

public class MsgHardware : ClientMsg, IClientMsg {
    private readonly string _commandStr;

    public MsgHardware(string commandStr) {
        _commandStr = commandStr;

        if (commandStr.Length > 2) {
            _ = commandStr[1] switch {
                'U' => Add(new MessageEvent("UUID", commandStr[2..])),
                'M' => Add(new MessageEvent("Blocking Message", commandStr[2..])),
                'm' => Add(new MessageEvent("Minor Message", commandStr[2..])),
                'T' => Add(new MessageEvent("Hardware Manufacturer", commandStr[2..])),
                't' => Add(new MessageEvent("Hardware SubType", commandStr[2..])),
                _   => null
            };
        }
    }

    public override string ToString() {
        return $"MSG:Hardware => {_commandStr}";
    }
}