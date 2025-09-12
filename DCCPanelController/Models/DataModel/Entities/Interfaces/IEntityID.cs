namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IEntityID {
    public string Id { get; set; }
    public List<IEntityID> AllIDs();
}

public interface IEntityGeneratingID : IEntityID {
    public string NextID();
}