
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PointsEntity : Entity, IDrawingEntity {
    public override string EntityName => "Connection Points";
    
    [JsonConstructor]
    public PointsEntity() {}
    public PointsEntity(Panel panel) : this() {
        Parent = panel;
    }
    public PointsEntity(PointsEntity entity) : base(entity) {}
    public override Entity Clone() => new PointsEntity(this);
}