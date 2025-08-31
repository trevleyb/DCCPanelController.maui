using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class RightTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    [JsonConstructor]
    public RightTurnoutEntity() { }

    public RightTurnoutEntity(Panel panel) : base(panel) { }
    public RightTurnoutEntity(RightTurnoutEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.RightTurnoutTrack;
    public override string EntityName => "Right Turnout";

    public override Entity Clone() {
        return new RightTurnoutEntity(this);
    }
}