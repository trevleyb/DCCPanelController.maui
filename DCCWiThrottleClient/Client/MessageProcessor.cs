using DCCWiThrottleClient.Client.Messages;

namespace DCCWiThrottleClient.Client;

public class MessageProcessor(Turnouts? turnouts) {
    /// <summary>
    ///     Simply, given an input string, this will return a Command Object that
    ///     needs to be managed and processed based on the commandStr provided.
    /// </summary>
    public IClientMsg Interpret(string commandStr) {
        // When we get here, there will only ever be a single message as
        // we filter out in the server when we read each line.
        // ------------------------------------------------------------------
        if (!string.IsNullOrEmpty(commandStr) && commandStr.Length >= 1) {
            var clientMsg = DetermineClientMsgType();
            clientMsg.Process(commandStr);
            return clientMsg;
        }

        IClientMsg DetermineClientMsgType() {
            if (string.IsNullOrEmpty(commandStr)) return new MsgIgnore();
            var commandChar = (int)commandStr[0];
            var commandType = Enum.IsDefined(typeof(CommandTypeEnum), commandChar) ? (CommandTypeEnum)commandChar : CommandTypeEnum.Ignore;

            return commandType switch {
                CommandTypeEnum.Name          => new MsgName(),
                CommandTypeEnum.Hardware      => new MsgHardware(),
                CommandTypeEnum.MultiThrottle => new MsgMultiThrottle(),
                CommandTypeEnum.Panel         => new MsgPanel(turnouts),
                CommandTypeEnum.Roster        => new MsgRoster(),
                CommandTypeEnum.Heartbeat     => new MsgHeartbeat(),
                CommandTypeEnum.Quit          => new MsgQuit(),
                _                             => new MsgIgnore()
            };
        }

        return new MsgIgnore();
    }
}

public enum CommandTypeEnum {
    Name          = 'N',
    Hardware      = 'H',
    MultiThrottle = 'M',
    Panel         = 'P',
    Roster        = 'R',
    Heartbeat     = '*',
    Quit          = 'Q',
    Ignore        = 'X'
}