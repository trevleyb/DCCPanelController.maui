namespace DCCClients.Jmri.JMRI.EventArgs;

public class RouteEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the route.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     The current state of the route (e.g., "ACTIVE", "INACTIVE").
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    ///     The list of associated turnouts and their states.
    /// </summary>
    public Dictionary<string, string> TurnoutStates { get; set; } = new();

    /// <summary>
    ///     Additional metadata or information about the route.
    /// </summary>
    public string? Metadata { get; set; }
}