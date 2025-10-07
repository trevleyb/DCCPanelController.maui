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
        "A **crossing** is a 4-way crossing track that can be straight or angled. ";
    
    public override Entity Clone() => new CrossingEntity(this);
}