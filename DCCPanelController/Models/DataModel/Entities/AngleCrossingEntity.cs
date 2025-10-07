using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class AngleCrossingEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public AngleCrossingEntity() { }

    public AngleCrossingEntity(Panel panel) : this() => Parent = panel;

    public AngleCrossingEntity(AngleCrossingEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => 
        Rotation % 90 == 0 ? EntityConnections.TrackPatterns.AngleCrossingTrack1 : EntityConnections.TrackPatterns.AngleCrossingTrack2;
    
    [JsonIgnore] public override string EntityName => "Angle";
    [JsonIgnore] public override string EntityDescription => "45-degree Crossing Track";
    [JsonIgnore] public override string EntityInformation => 
        "The Angle Crossing Track (**Angle Crossing**) is a track that crosses in the middle and is at 45 degrees. ";
    
    public override Entity Clone() => new AngleCrossingEntity(this);
}