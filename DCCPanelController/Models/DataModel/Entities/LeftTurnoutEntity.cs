using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LeftTurnoutEntity : TurnoutEntity {
    public override string Name => "Left Turnout";
    
    [JsonConstructor]
    private LeftTurnoutEntity() {}
    public LeftTurnoutEntity(Panel panel) : base(panel) { }
    public LeftTurnoutEntity(LeftTurnoutEntity entity) : base(entity) {}
    public override Entity Clone() => new LeftTurnoutEntity(this);
}