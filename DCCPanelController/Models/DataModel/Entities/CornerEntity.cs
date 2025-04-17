using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CornerEntity : TrackEntity, ITrackEntity {
    public override string EntityName => "Corner Track";
    
    [JsonConstructor]
    public CornerEntity() {}
    public CornerEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CornerEntity(CornerEntity entity) : base(entity) {}
    public override Entity Clone() => new CornerEntity(this);
}