using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class CornerEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public CornerEntity() { }

    public CornerEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CornerEntity(CornerEntity entity) : base(entity) { }
    public override string EntityName => "Corner Track";

    public override Entity Clone() {
        return new CornerEntity(this);
    }
}