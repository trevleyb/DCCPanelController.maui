using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CrossingEntity : Entity {
    public override string Name => "Crossing Track";

    [JsonConstructor]
    private CrossingEntity() {}
    public CrossingEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CrossingEntity(CrossingEntity entity) : base(entity) {}
    public override Entity Clone() => new CrossingEntity(this);
}