using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class StraightEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public StraightEntity() => Layer = 6; // Override the Straight Entity so it overlays anything else

    public StraightEntity(Panel panel) : this() => Parent = panel;

    public StraightEntity(StraightEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.StraightTrack;
    public override string EntityName => "Track";
    public override string EntityDescription => "Straight Track";
    public override string EntityInformation =>
        "The **straight** is a straight piece of track .";

    [JsonIgnore] protected override int RotationFactor => 45;

    public override Entity Clone() => new StraightEntity(this);
}