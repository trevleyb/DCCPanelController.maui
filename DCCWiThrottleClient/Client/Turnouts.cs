using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DCCWiThrottleClient.Client;

public class Turnouts : ObservableCollection<Turnout> {

    private int count;
    public Turnouts() {
        this.CollectionChanged += TurnoutsOnCollectionChanged;
    }

    private void TurnoutsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        count++;
    }

    public Turnout Find(string systemName) {
        if (string.IsNullOrEmpty(systemName)) throw new ArgumentNullException(nameof(systemName));
        if (this.FirstOrDefault(t => t.Name == systemName) == default) Add(systemName, "Unknown", "1");
        return this.First(t => t.Name == systemName);
    }

    public void Add(string systemName, string userName, StateEnum state) {
        this.Add(new Turnout(systemName, userName, state));
        //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
    }

    public void Add(string systemName, string userName, string state) {
        var stateEnum = StateEnum.Unknown;
        if (state.Length >= 1) {
            stateEnum = state[0] switch {
                '1' => StateEnum.Unknown,
                '2' or 'C' => StateEnum.Closed,
                '4' or 'T' => StateEnum.Thrown,
                _   => StateEnum.Unknown
            };
        }
        this.Add(new Turnout(systemName, userName, stateEnum));
        //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
    }

    public Turnout UpdateFromWiThrottle(string commandStr) {
        var turnout = new Turnout("Unknown", "Unknown", StateEnum.Unknown);
        try {
            var thrownOrClosed = commandStr[0];
            var turnoutName    = commandStr[1..];
            turnout = Find(turnoutName);
            turnout.StateEnum = thrownOrClosed switch {
                'C' or '2' => StateEnum.Closed,
                'T' or '4' => StateEnum.Thrown,
                _          => StateEnum.Unknown
            };
        } catch {
            turnout = new Turnout("Unknown", "Unknown", StateEnum.Unknown);
        }
        //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
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