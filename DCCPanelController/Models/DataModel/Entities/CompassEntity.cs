using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Entities;

public class CompassEntity : Entity {
    public CompassEntity() { }

    public CompassEntity(Panel panel) : this() => Parent = panel;

    public CompassEntity(CompassEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;
    public override string EntityName => "Compass";
    public override string EntityDescription => "Directional Compass";
    public override string EntityInformation => "";

    public override Entity Clone() => new CompassEntity(this);
}