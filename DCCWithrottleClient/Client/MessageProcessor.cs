using DCCWithrottleClient.Client.Entities;
using DCCWithrottleClient.Client.Messages;

namespace DCCWithrottleClient.Client;

public static class MessageProcessor {
    /// <summary>
    ///     Simply, given an input string, this will return a Command Object that
    ///     needs to be managed and processed based on the commandStr provided.
    /// </summary>
    public static IClientMsg Interpret(string commandStr) {
        // When we get here, there will only ever be a single message as
        // we filter out in the server when we read each line.
        // ------------------------------------------------------------------
        if (!string.IsNullOrEmpty(commandStr) && commandStr.Length >= 1) {
            if (string.IsNullOrEmpty(commandStr)) return new MsgIgnore(commandStr);

            var commandChar = (int)commandStr[0];
            var commandType = Enum.IsDefined(typeof(CommandTypeEnum), commandChar) ? (CommandTypeEnum)commandChar : CommandTypeEnum.Ignore;

            return commandType switch {
                CommandTypeEnum.Name          => new MsgName(commandStr),
                CommandTypeEnum.Hardware      => new MsgHardware(commandStr),
                CommandTypeEnum.MultiThrottle => new MsgMultiThrottle(commandStr),
                CommandTypeEnum.Panel         => new MsgPanel(commandStr),
                CommandTypeEnum.Roster        => new MsgRoster(commandStr),
                CommandTypeEnum.Heartbeat     => new MsgHeartbeat(commandStr),
                CommandTypeEnum.Quit          => new MsgQuit(commandStr),
                _                             => new MsgIgnore(commandStr)
            };
        }
        return new MsgIgnore(commandStr);
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