namespace DCCPanelController.Models.DataModel;

public interface IDccTable {
    string? Id { get; set; }
    string? Name { get; set; }
    int? DccAddress { get; set; }
    bool IsEditable { get; set; }
    bool IsModified { get; set; }
    
    string DisplayFormat { get; }
}