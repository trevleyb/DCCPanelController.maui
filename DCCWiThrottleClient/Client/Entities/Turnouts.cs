using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DCCWiThrottleClient.Client;

public class Turnouts : EntityCollectionBase<Turnout> {

    public void Add(string systemName, string userName, TurnoutStateEnum turnoutState) {
        Add(new Turnout(systemName, userName, turnoutState));
    }

    public void Add(string systemName, string userName, string state) {
        var stateEnum = TurnoutStateEnum.Unknown;
        if (state.Length >= 1) {
            stateEnum = state[0] switch {
                '1' => TurnoutStateEnum.Unknown,
                '2' or 'C' => TurnoutStateEnum.Closed,
                '4' or 'T' => TurnoutStateEnum.Thrown,
                _   => TurnoutStateEnum.Unknown
            };
        }
        Add(new Turnout(systemName, userName, stateEnum));
    }

    public Turnout UpdateFromWiThrottle(string commandStr) {
        var turnout = new Turnout("Unknown", "Unknown", TurnoutStateEnum.Unknown);
        try {
            var thrownOrClosed = commandStr[0];
            var turnoutName    = commandStr[1..];
            turnout = Find(turnoutName);
            turnout.StateEnum = thrownOrClosed switch {
                'C' or '2' => TurnoutStateEnum.Closed,
                'T' or '4' => TurnoutStateEnum.Thrown,
                _          => TurnoutStateEnum.Unknown
            };
        } catch (Exception ex) {
            turnout = new Turnout("Unknown", "Unknown", TurnoutStateEnum.Unknown);
        }
        return turnout;
    }
    
    public void AddFromWiThrottle(string commandStr) {
        // eg: PTL]\[LT304}|{Yard Entry}|{2]\[LT305}|{A/D Track}|{4
        try {
            var entries = commandStr.Split("]\\[");
            foreach (var entry in entries) {
                if (string.IsNullOrEmpty(entry)) continue;
                var parts      = entry.Split("}|{");
                if (parts.Length == 3) {
                    var systemName = parts[0];
                    var userName   = parts[1];
                    var state      = parts[2];
                    Add(systemName, userName, state);
                }
            }
        } catch { /* ignore any issues */
        }
    }
}