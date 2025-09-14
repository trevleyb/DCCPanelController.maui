using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class CrossingEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public CrossingEntity() { }

    public CrossingEntity(Panel panel) : this() => Parent = panel;

    public CrossingEntity(CrossingEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.CrossingTrack;
    public override string EntityName => "Crossing";
    public override string EntityDescription => "90-degree Crossing Track";
    public override string EntityInformation => "";

    public override Entity Clone() => new CrossingEntity(this);
}