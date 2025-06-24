using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Entities;

public class CompassEntity : Entity {
    public CompassEntity() { }

    public CompassEntity(Panel panel) : this() {
        Parent = panel;
    }

    [JsonIgnore] protected override int RotationFactor => 90;
    
    public CompassEntity(CompassEntity entity) : base(entity) { }
    public override string EntityName => "Compass";

    public override Entity Clone() {
        return new CompassEntity(this);
    }
}