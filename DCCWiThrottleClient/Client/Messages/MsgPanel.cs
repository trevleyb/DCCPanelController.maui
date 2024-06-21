namespace DCCWiThrottleClient.Client.Messages;

public class MsgPanel(Turnouts? turnouts) : IClientMsg {
    public void Process(string commandStr) {
        var command = commandStr[0..3];
        switch (command) {
        case "PTL":
            ActionTaken = "Processed Turnout List";
            turnouts?.AddFromWiThrottle(commandStr[3..]);
            break;
        case "PTA":
            var turnout = turnouts?.UpdateFromWiThrottle(commandStr[3..]);
            ActionTaken = "Processed Turnout:" + turnout?.Name ?? "Unknown" + "=>" + turnout?.State.ToString() ?? "Unknown";
            break;
        case "PFT":
            var fastClock = GetFastClock(commandStr[3..]);
            ActionTaken = "FastClock:" + fastClock.TimeOfDay.ToString();
            break;
        }
    }

    public override string ToString() {
        return "MSG:Panel";
    }

    public string ActionTaken { get; private set; } = string.Empty;

    private DateTime GetFastClock(string commandStr) {
        int fastClockNum = 0;
        try {
            var fastClockStr                                                = commandStr[3..][..commandStr[3..].IndexOf("<;>", StringComparison.Ordinal)];
            if (!int.TryParse(fastClockStr, out fastClockNum)) fastClockNum = 0;
        } catch {
            fastClockNum = 0;
        }
        return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(fastClockNum);
        
    }
}

/*
   • L - Turnout list with a format of ]\[ system name }|{ user name }|{ state. This
   pattern repeats for each turnout stored in the server that is available to the client.
   PTL]\[LT304}|{Yard Entry}|{2]\[LT305}|{A/D Track}|{4
   • A - Action. Has two patterns:
   1. Sent to server - commands are 2 (toggle), C (closed), or T (thrown).
   PTA, followed by command, followed by system name.
   PTATLT304
   Set “thrown” LT304.
   2. Sent to client - States are 1 (unknown), 2 (closed), or 4 (thrown). Server should
   send state to clients whenever state changes.
   PTA, followed by state, followed by system name.
   PTA4LT304
   State of LT304 is “thrown”.

   */