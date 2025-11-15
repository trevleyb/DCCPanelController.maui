namespace DCCPanelController.Models.DataModel.Accessories;

/// <summary>
/// How this accessory will be addressed on the current connection.
/// </summary>
public enum AccessoryBindingMode {
    /// <summary>
    /// Not currently bound for this connection.
    /// </summary>
    Unbound = 0,

    /// <summary>
    /// Use the SystemId string as-is (e.g. "NT432", "LT12").
    /// </summary>
    SystemId = 1,

    /// <summary>
    /// Use the numeric DCC address as a bare numeric name (e.g. "432").
    /// </summary>
    NumericDccAddress = 2
}
