using DCCClients.Common;

namespace DCCClients.JMRI.EventArgs;

public class SignalEventArgs : System.EventArgs {
    /// <summary>
    ///     The unique identifier of the signal.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    ///     The DCC address of the signal, if applicable.
    /// </summary>
    public int? DccAddress { get; set; }

    /// <summary>
    ///     The current aspect of the signal.
    /// </summary>
    public SignalAspectEnum Aspect { get; set; } = SignalAspectEnum.Off;

    /// <summary>
    ///     The raw state string from JMRI, if available.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    ///     Additional metadata or information about the signal.
    /// </summary>
    public string? Metadata { get; set; }
}
