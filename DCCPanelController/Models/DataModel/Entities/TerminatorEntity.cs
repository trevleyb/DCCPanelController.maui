using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TerminatorEntity : TrackEntity, ITrackEntity {
    public override string EntityName => "Terminator Track";

    [JsonConstructor]
    public TerminatorEntity() { }
    public TerminatorEntity(Panel panel) : this() {
        Parent = panel;
    }
    public TerminatorEntity(TerminatorEntity entity) : base(entity) { }
    public override Entity Clone() => new TerminatorEntity(this);
}