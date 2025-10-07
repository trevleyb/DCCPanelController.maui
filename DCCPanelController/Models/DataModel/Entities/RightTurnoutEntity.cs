using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class RightTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    [JsonConstructor]
    public RightTurnoutEntity() { }

    public RightTurnoutEntity(Panel panel) : base(panel) { }
    public RightTurnoutEntity(RightTurnoutEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => 
        Rotation % 90 == 0 ? EntityConnections.TrackPatterns.RightTurnoutTrack : EntityConnections.TrackPatterns.RightAngleTurnoutTrack;

    [JsonIgnore] public override string EntityName => "RTurnout";
    [JsonIgnore] public override string EntityName => "Right Turnout/Switch";
    public override string EntityInformation =>
        "The *Right turnout* is a switch or turnout that shows the track as **straight** *(normal)* or **diverging** *(reversed)*. This is an interactive track that allows you to control other buttons or tracks based on the state of the turnout." ;

    public override Entity Clone() => new RightTurnoutEntity(this);
}