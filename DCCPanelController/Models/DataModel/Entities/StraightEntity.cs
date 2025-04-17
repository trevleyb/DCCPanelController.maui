
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightEntity : TrackEntity, ITrackEntity {
    public override string EntityName => "Straight Track";

    [JsonConstructor]
    public StraightEntity() {
        Layer = 6;  // Override the Straight Entity so it overlays anything else
    }
    public StraightEntity(Panel panel) : this() {
        Parent = panel;
    }
    public StraightEntity(StraightEntity entity) : base(entity) {}
    public override Entity Clone() => new StraightEntity(this);
}