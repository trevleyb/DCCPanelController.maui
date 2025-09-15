using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class LeftTurnoutEntity : TurnoutEntity, ITrackEntity, IInteractiveEntity {
    [JsonConstructor]
    public LeftTurnoutEntity() { }

    public LeftTurnoutEntity(Panel panel) : base(panel) { }
    public LeftTurnoutEntity(LeftTurnoutEntity entity) : base(entity) { }
    
    [JsonIgnore] public override EntityConnections Connections =>
        Rotation % 90 == 0 ? EntityConnections.TrackPatterns.LeftTurnoutTrack 
            : EntityConnections.TrackPatterns.LeftAngleTurnoutTrack;

    public override string EntityName => "LTurnout";
    public override string EntityDescription => "Left Turnout/Switch";
    public override string EntityInformation =>
        "The *Left turnout* is a switch or turnout that shows the track as **straight** *(normal)* or **diverging** *(reversed)*. This is an interactive track that allows you to control other buttons or tracks based on the state of the turnout." ;

    public override Entity Clone() => new LeftTurnoutEntity(this);
}