using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class LeftTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    public override string EntityName => "Left Turnout";
    
    [JsonConstructor]
    public LeftTurnoutEntity() {}
    public LeftTurnoutEntity(Panel panel) : base(panel) { }
    public LeftTurnoutEntity(LeftTurnoutEntity entity) : base(entity) {}
    public override Entity Clone() => new LeftTurnoutEntity(this);
}