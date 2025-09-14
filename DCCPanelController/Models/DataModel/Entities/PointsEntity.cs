using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class PointsEntity : Entity, IDrawingEntity {
    [JsonConstructor]
    public PointsEntity() { }

    public PointsEntity(Panel panel) : this() => Parent = panel;

    public PointsEntity(PointsEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Points";
    public override string EntityDescription => "Connection Points";
    public override string EntityInformation => "";

    public override Entity Clone() => new PointsEntity(this);
}