using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities.Interfaces;

public interface IEntityID {
    public EntityIDField Id { get; set; }
    public string NextID { get; }
    public List<IEntityID> AllIDs { get; }
}