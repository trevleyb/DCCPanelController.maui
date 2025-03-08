
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightEntity : Entity{
    public override string Name => "Straight Track";
    
    [JsonConstructor]
    private StraightEntity() {}
    public StraightEntity(Panel panel) : this() {
        Parent = panel;
    }
    public StraightEntity(StraightEntity entity) : base(entity) {}
    public override Entity Clone() => new StraightEntity(this);
}