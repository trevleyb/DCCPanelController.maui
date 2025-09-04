using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class LeftTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    [JsonConstructor]
    public LeftTurnoutEntity() { }

    public LeftTurnoutEntity(Panel panel) : base(panel) { }
    public LeftTurnoutEntity(LeftTurnoutEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.LeftTurnoutTrack;
    public override string EntityName => "LTurnout";
    public override string EntityDescription => "Left Turnout/Switch";
    
    public override Entity Clone() {
        return new LeftTurnoutEntity(this);
    }
}