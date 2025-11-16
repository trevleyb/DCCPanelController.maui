namespace DCCPanelController.Models.DataModel.Accessories;

/// <summary>
/// Origin or primary source of this accessory definition.
/// </summary>
public enum AccessorySource {
    Unknown     = 0,
    Jmri        = 1,
    WiThrottle  = 2,
    Manual      = 3,
    Imported    = 4
}