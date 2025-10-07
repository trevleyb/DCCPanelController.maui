using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Entities;

public class CompassEntity : Entity {
    public CompassEntity() { }

    public CompassEntity(Panel panel) : this() => Parent = panel;

    public CompassEntity(CompassEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;
    [JsonIgnore] public override string EntityName => "Compass";
    [JsonIgnore] public override string EntityName => "Directional Compass";
    public override string EntityInformation =>
        "A simple compass which shows the direction of travel";
    
    public override Entity Clone() => new CompassEntity(this);
}