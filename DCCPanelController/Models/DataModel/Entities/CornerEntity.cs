using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class CornerEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public CornerEntity() { }

    public CornerEntity(Panel panel) : this() => Parent = panel;

    public CornerEntity(CornerEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.CornerTrack;
    [JsonIgnore] public override string EntityName => "Corner";
    [JsonIgnore] public override string EntityDescription => "Corner Track";
    [JsonIgnore] public override string EntityInformation =>
        "The Corner Track is a track to turn a 45 degree corner. You need to connect 2 corner tracks for a 90 degree turn. ";

    public override Entity Clone() => new CornerEntity(this);
}