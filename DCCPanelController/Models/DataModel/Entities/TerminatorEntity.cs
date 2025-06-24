using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.Models.DataModel.Entities;

public class TerminatorEntity : TrackEntity, ITrackEntity {
    [JsonConstructor]
    public TerminatorEntity() { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.TerminatorTrack;
    
    public TerminatorEntity(Panel panel) : this() {
        Parent = panel;
    }

    public TerminatorEntity(TerminatorEntity entity) : base(entity) { }
    public override string EntityName => "Terminator (Buffer Stop) Track";

    public override Entity Clone() {
        return new TerminatorEntity(this);
    }
}