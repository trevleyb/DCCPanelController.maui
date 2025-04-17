using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class PlatformEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public PlatformEntity() { }

    public PlatformEntity(Panel panel) : this() {
        Parent = panel;
    }

    public PlatformEntity(PlatformEntity entity) : base(entity) { }
    public override string EntityName => "Straight Track";

    public override Entity Clone() {
        return new PlatformEntity(this);
    }
}