using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CompassEntity : Entity {
    public override string Name => "Compass";

    public CompassEntity() {}
    public CompassEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CompassEntity(CompassEntity entity) : base(entity) {}
    public override Entity Clone() => new CompassEntity(this);
}