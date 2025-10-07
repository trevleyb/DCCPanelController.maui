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
    [JsonIgnore] public override string EntityName => "Crossing";
    [JsonIgnore] public override string EntityDescription => "90-degree Crossing Track";
    [JsonIgnore] public override string EntityInformation => 
        "The Crossing Track (**Crossing**) is a track that crosses in the middle and is at 90 degrees. ";
    
    public override Entity Clone() => new CrossingEntity(this);
}