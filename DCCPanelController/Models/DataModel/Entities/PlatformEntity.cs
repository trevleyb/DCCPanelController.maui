
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class PlatformEntity : TrackEntity, ITrackEntity {
    public override string EntityName => "Straight Track";
    
    [JsonConstructor]
    public PlatformEntity() {}
    public PlatformEntity(Panel panel) : this() {
        Parent = panel;
    }
    public PlatformEntity(PlatformEntity entity) : base(entity) {}
    public override Entity Clone() => new PlatformEntity(this);
}