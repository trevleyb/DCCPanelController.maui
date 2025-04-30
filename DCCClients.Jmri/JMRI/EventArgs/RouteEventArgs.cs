using System.Data;
using DCCClients.Jmri.JMRI.DataBlocks;

namespace DCCClients.Jmri.JMRI.EventArgs;

public class RouteEventArgs : System.EventArgs {
    public string Identifier { get; set; }
    public string State { get; set; }
    public Dictionary<string, string> TurnoutStates { get; set; } = new();
    public string? Metadata { get; set; }
    
    public RouteEventArgs(string identifier, string state, Dictionary<string, string> turnoutStates, string? metadata) {
        Identifier = identifier;
        State = state;
        TurnoutStates = turnoutStates;
        Metadata = metadata;
    }
    
    public RouteEventArgs(string jsonString) {
        var routeData = RouteParser.ParseRouteData(jsonString);
        if (routeData is null) throw new DataException("Invalid JSON object for Route: " + jsonString);
        Identifier =routeData.Data.UserName;
        State = routeData.Data.State == 0 ? "ACTIVE" : "INACTIVE";
    }

}