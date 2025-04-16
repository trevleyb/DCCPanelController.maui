using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class RightTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    public override string EntityName => "Right Turnout";
    
    [JsonConstructor]
    public RightTurnoutEntity() {}
    public RightTurnoutEntity(Panel panel) : base(panel) { }
    public RightTurnoutEntity(RightTurnoutEntity entity) : base(entity) {}
    public override Entity Clone() => new RightTurnoutEntity(this);
}