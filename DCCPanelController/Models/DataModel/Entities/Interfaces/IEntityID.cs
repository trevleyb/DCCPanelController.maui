namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IEntityID {
    public string Id { get; set; }
}

public interface IEntityGeneratingID : IEntityID {
    public string NextID(Panel? targetPanel = null);
}