namespace DCCPanelController.Models.DataModel;

public interface ITable {
    string? Id { get; set; }
    string? Name { get; set; }
    bool IsEditable { get; set; }
    bool IsModified { get; set; }
}