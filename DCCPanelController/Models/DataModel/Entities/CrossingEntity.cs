using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CrossingEntity : TrackEntity, ITrackEntity {
    public override string EntityName => "Crossing Track";

    [JsonConstructor]
    public CrossingEntity() {}
    public CrossingEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CrossingEntity(CrossingEntity entity) : base(entity) {}
    public override Entity Clone() => new CrossingEntity(this);
}