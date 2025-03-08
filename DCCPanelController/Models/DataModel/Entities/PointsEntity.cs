
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PointsEntity : Entity {
    public override string Name => "Connection Points";
    
    [JsonConstructor]
    private PointsEntity() {}
    public PointsEntity(Panel panel) : this() {
        Parent = panel;
    }
    public PointsEntity(PointsEntity entity) : base(entity) {}
    public override Entity Clone() => new PointsEntity(this);
}