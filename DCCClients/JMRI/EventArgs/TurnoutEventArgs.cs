namespace DCCJmriClient.EventArgs;

public class TurnoutEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the turnout.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     The DCC address of the turnout, if applicable.
    /// </summary>
    public int? DccAddress { get; set; }

    /// <summary>
    ///     The current state of the turnout (e.g., "THROWN", "CLOSED").
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the turnout is locked.
    /// </summary>
    public bool IsLocked { get; set; }
}