using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CornerContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableEnum("Continuation", "Page Continuation Indicator", 5, "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public CornerContinuationEntity() { }

    public CornerContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CornerContinuationEntity(CornerContinuationEntity entity) : base(entity) { }

    [JsonIgnore] public override EntityConnections Connections => EntityConnections.TrackPatterns.CornerContinuationTrack;

    public override string EntityName => "Corner Continuation Track";

    public override Entity Clone() {
        return new CornerContinuationEntity(this);
    }
}