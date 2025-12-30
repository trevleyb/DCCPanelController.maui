using System.Diagnostics;
using DCCPanelController.Clients.WiThrottle.Events;
using DCCPanelController.Models.DataModel.Accessories;

namespace DCCPanelController.Clients.WiThrottle.Messages;

public class MsgPanel : ClientMsg, IClientMsg {
    private readonly string _commandStr;

    public MsgPanel(string commandStr) {
        _commandStr = commandStr;

        var command = commandStr[..3];

        switch (command) {
        // Panel Turnout List 
        // eg: PTL]\[LT304}|{Yard Entry}|{2]\[LT305}|{A/D Track}|{4
        // ----------------------------------------------------------------------------------------
        case "PTL":
            try {
                var entries = commandStr.Split("]\\[");
                foreach (var entry in entries) {
                    if (string.IsNullOrEmpty(entry)) continue;
                    var parts = entry.Split("}|{");
                    if (parts.Length == 3) Add(new TurnoutEvent(parts[0], parts[1], parts[2], AccessorySource.WiThrottle));
                }
            } catch (Exception ex) {
                Trace.WriteLine($"Error parsing turnout list: {ex.Message}");
            }

            break;

        case "PTA":
            try {
                var thrownOrClosed = commandStr[3];
                var turnoutName = commandStr[4..];
                Add(new TurnoutEvent(turnoutName, thrownOrClosed));
            } catch (Exception ex) {
                Trace.WriteLine($"Error parsing turnout action: {ex.Message}");
            }

            break;

        // PanelRoutes List 
        // ----------------------------------------------------------------------------------------
        case "PRL":
            try {
                var entries = commandStr.Split("]\\[");

                foreach (var entry in entries) {
                    if (string.IsNullOrEmpty(entry)) continue;
                    var parts = entry.Split("}|{");
                    if (parts.Length == 3) Add(new RouteEvent(parts[0], parts[1], parts[2]));
                }
            } catch (Exception ex) {
                Trace.WriteLine($"Error parsing routes list: {ex.Message}");
            }

            break;

        case "PRA":
            try {
                var activeOrInactive = commandStr[3];
                var routeName = commandStr[4..];
                Add(new RouteEvent(routeName, activeOrInactive));
            } catch (Exception ex) {
                Trace.WriteLine($"Error parsing route action: {ex.Message}");
            }

            break;

        case "PFT":
            var fastClock = GetFastClock(commandStr[3..]);
            Add(new FastClockEvent(fastClock, 0));
            break;

        case "PPA":
            var power = GetPowerState(commandStr[3..]);
            Add(new PowerEvent(power));
        break;
        }
    }

    public override string ToString() {
        return $"MSG:Panel => {_commandStr}";
    }

    /// <summary>
    ///     Convert the string data from the command into the current fast clock Date time
    /// </summary>
    /// <param name="commandStr"></param>
    /// <returns></returns>
    private DateTime GetFastClock(string commandStr) {
        var fastClockNum = 0;
        try {
            var fastClockStr = commandStr[3..][..commandStr[3..].IndexOf("<;>", StringComparison.Ordinal)];
            if (!int.TryParse(fastClockStr, out fastClockNum)) fastClockNum = 0;
        } catch {
            fastClockNum = 0;
        }

        return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(fastClockNum);
    }

    private int GetPowerState(string commandStr) {
        try {
            var stateStr = commandStr[3].ToString();
            return int.TryParse(stateStr, out var state) ? state : 0;
        } catch {
            return 0;
        }
        
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