namespace DCCPanelController.Models.DataModel.Interfaces;

public interface IEntityID {
    public string Id { get; set; }
    public string GenerateID();
}
