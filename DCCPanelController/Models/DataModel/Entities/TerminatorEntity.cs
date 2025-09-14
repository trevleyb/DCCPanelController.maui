using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class TerminatorEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public TerminatorEntity() { }

    public TerminatorEntity(Panel panel) : this() => Parent = panel;

    public TerminatorEntity(TerminatorEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.TerminatorTrack;
    public override string EntityName => "Terminator";
    public override string EntityDescription => "Terminator or Buffer-Stop Track";
    public override string EntityInformation => "";

    public override Entity Clone() => new TerminatorEntity(this);
}