using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CornerEntity : Entity {
    public override string Name => "Corner Track";
    
    [JsonConstructor]
    private CornerEntity() {}
    public CornerEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CornerEntity(CornerEntity entity) : base(entity) {}
    public override Entity Clone() => new CornerEntity(this);
}