namespace DCCPanelController.Models.DataModel.Accessories;

/// <summary>
/// Origin or primary source of this accessory definition.
/// </summary>
public enum AccessorySource {
    Unknown     = 0,
    Manual      = 1,
    Jmri        = 2,
    WiThrottle  = 3,
    Simulator   = 4,
}