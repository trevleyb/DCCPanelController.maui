using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class PointsEntity : Entity, IDrawingEntity {
    [JsonConstructor]
    public PointsEntity() { }

    public PointsEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;

    public PointsEntity(PointsEntity entity) : base(entity) { }
    public override string EntityName => "Connection Points";

    public override Entity Clone() {
        return new PointsEntity(this);
    }
}