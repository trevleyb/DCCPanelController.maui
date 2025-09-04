using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableEnum("Continuation", "Page Continuation Indicator", 5, "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public StraightContinuationEntity() { }

    public StraightContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }

    public StraightContinuationEntity(StraightContinuationEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.StraightContinuationTrack;

    public override string EntityName => "Straight...";
    public override string EntityDescription => "Straight Track with Indicator";
    
    public override Entity Clone() {
        return new StraightContinuationEntity(this);
    }
}