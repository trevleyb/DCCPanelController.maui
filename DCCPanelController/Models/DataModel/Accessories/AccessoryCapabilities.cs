namespace DCCPanelController.Models.DataModel.Accessories;

/// <summary>
/// Describes how a particular WiThrottle-like server treats accessory IDs.
/// You can configure this per server type (JMRI, WFD31, etc).
/// </summary>
public sealed class AccessoryCapabilities {
    /// <summary>
    /// True if this server accepts a purely numeric "name" for PTA/route commands.
    /// </summary>
    public bool SupportsNumericTurnoutName { get; init; }

    /// <summary>
    /// True if, on this server, a numeric "name" is interpreted as a DCC/accessory address.
    /// Only set this to true if you *know* that is how the backend behaves.
    /// </summary>
    public bool NumericNameIsDccAddress { get; init; }

    public static readonly AccessoryCapabilities Default =
        new AccessoryCapabilities {
            SupportsNumericTurnoutName = false,
            NumericNameIsDccAddress = false
        };
}
