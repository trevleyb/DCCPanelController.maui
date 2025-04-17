using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public class TerminatorEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public TerminatorEntity() { }

    public TerminatorEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TerminatorEntity(TerminatorEntity entity) : base(entity) { }
    public override string EntityName => "Terminator Track";

    public override Entity Clone() {
        return new TerminatorEntity(this);
    }
}