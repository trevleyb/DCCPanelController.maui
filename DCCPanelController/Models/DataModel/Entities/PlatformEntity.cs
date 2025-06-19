using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class PlatformEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public PlatformEntity() { }

    public override EntityConnections Connections => EntityConnections.TrackPatterns.PlatformTrack;
    public PlatformEntity(Panel panel) : this() {
        Parent = panel;
    }

    public PlatformEntity(PlatformEntity entity) : base(entity) { }
    public override string EntityName => "Straight Track";

    public override Entity Clone() {
        return new PlatformEntity(this);
    }
}