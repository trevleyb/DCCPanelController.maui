using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CornerContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("Continuation", "Page Continuation Indicator", 5, "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public CornerContinuationEntity() { }

    public CornerContinuationEntity(Panel panel) : this() => Parent = panel;

    public CornerContinuationEntity(CornerContinuationEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.CornerContinuationTrack;

    public override string EntityName => "Corner...";
    public override string EntityDescription => "Corner Track with Indicator";
    public override string EntityInformation =>
        "The **corner indicator** is a track that turns 45 degrees (2 to turn 90) but includes an end-indicator used to represent a continuation of the track to another page. *(future support to allow clicking to switch panels)*. You can use either an arrow indicator or a double-line indicator.";
    
    public override Entity Clone() => new CornerContinuationEntity(this);
}