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
        "The **corner** is a track that turns 45 degrees (2 to turn 90). ";

    public override Entity Clone() => new CornerEntity(this);
}