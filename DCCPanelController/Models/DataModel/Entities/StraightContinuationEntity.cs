using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: Editable("Continuation", "Page Continuation Indicator", 5, "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public StraightContinuationEntity() { }

    public StraightContinuationEntity(Panel panel) : this() => Parent = panel;

    public StraightContinuationEntity(StraightContinuationEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.StraightContinuationTrack;

    public override string EntityName => "Straight...";
    public override string EntityDescription => "Straight Track with Indicator";
    public override string EntityInformation =>
        "The **straight indicator** is a straight piece of track but includes an end-indicator used to represent a continuation of the track to another page. *(future support to allow clicking to switch panels)*. You can use either an arrow indicator or a double-line indicator.";

    public override Entity Clone() => new StraightContinuationEntity(this);
}