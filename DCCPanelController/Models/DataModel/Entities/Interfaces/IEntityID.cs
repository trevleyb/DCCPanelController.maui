namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IEntityID {
    public string Id { get; set; }
    public string NextID { get; }
    public List<IEntityID> AllIDs { get; }
}