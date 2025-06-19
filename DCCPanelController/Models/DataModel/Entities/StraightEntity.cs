using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class StraightEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public StraightEntity() {
        Layer = 6; // Override the Straight Entity so it overlays anything else
    }

    public override EntityConnections Connections => EntityConnections.TrackPatterns.StraightTrack;
    
    public StraightEntity(Panel panel) : this() {
        Parent = panel;
    }

    public StraightEntity(StraightEntity entity) : base(entity) { }
    public override string EntityName => "Straight Track";

    public override Entity Clone() {
        return new StraightEntity(this);
    }
}