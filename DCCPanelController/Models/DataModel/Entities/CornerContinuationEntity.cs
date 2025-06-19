using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CornerContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableEnum("Terminator", group: "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    public override EntityConnections Connections => EntityConnections.TrackPatterns.CornerContinuationTrack;
    [JsonConstructor]
    public CornerContinuationEntity() { }

    public CornerContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }

    public CornerContinuationEntity(CornerContinuationEntity entity) : base(entity) {
        ContinuationStyle = entity.ContinuationStyle;
    }

    public override string EntityName => "Corner Track";

    public override Entity Clone() {
        return new CornerContinuationEntity(this);
    }
}