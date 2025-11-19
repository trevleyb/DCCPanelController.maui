namespace DCCPanelController.Models.DataModel.Accessories;

public interface IAccessory {
    string? Id { get; set; }
    string? Name { get; set; }
    int? DccAddress { get; set; }
    bool IsValidForCurrentConnection { get; set; }

    AccessorySource Source { get; set; }
    AccessoryBindingMode BindingMode { get; set; }

    string DisplayFormat { get; }
}