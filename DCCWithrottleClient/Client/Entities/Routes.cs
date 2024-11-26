namespace DCCWithrottleClient.Client.Entities;

public class Routes : EntityCollectionBase<Route> {

    public void Add(string systemName, string userName, RouteStateEnum state) {
        this.Add(new Route(systemName, userName, state));
    }

    public void Add(string systemName, string userName, string state) {
        var stateEnum = RouteStateEnum.Unknown;
        if (state.Length >= 1) {
            stateEnum = state[0] switch {
                '1' => RouteStateEnum.Unknown,
                '2' or 'C' => RouteStateEnum.Active,
                '4' or 'T' => RouteStateEnum.Inactive,
                _   => RouteStateEnum.Unknown
            };
        }
        Add(new Route(systemName, userName, stateEnum));
    }

    public Route UpdateFromWiThrottle(string commandStr) {
        var route = new Route("Unknown", "Unknown", RouteStateEnum.Unknown);
        try {
            var thrownOrClosed = commandStr[0];
            var turnoutName    = commandStr[1..];
            route = Find(turnoutName);
            route.StateEnum = thrownOrClosed switch {
                'C' or '2' => RouteStateEnum.Active,
                'T' or '4' => RouteStateEnum.Inactive,
                _          => RouteStateEnum.Unknown
            };
        } catch {
            route = new Route("Unknown", "Unknown", RouteStateEnum.Unknown);
        }
        return route;
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