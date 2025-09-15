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
    
    public override string EntityName => "Angle";
    public override string EntityDescription => "45-degree Crossing Track";
    public override string EntityInformation => 
        "A **crossing** track where one side is 45 degree and the other is straight";
    
    public override Entity Clone() => new AngleCrossingEntity(this);
}