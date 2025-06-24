using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IEntityID {
    public string Id { get; set; }
}

public interface IEntityGeneratingID : IEntityID {
    public string NextID { get; }
    public List<IEntityID> AllIDs { get; }
}