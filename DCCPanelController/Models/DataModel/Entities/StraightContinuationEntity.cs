using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightContinuationEntity : TrackEntity, ITrackEntity {
    public override string Name => "Straight Track";
    [ObservableProperty] private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public StraightContinuationEntity() {}
    public StraightContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }
    public StraightContinuationEntity(StraightContinuationEntity entity) : base(entity) {
        ContinuationStyle = entity.ContinuationStyle;
    }
    public override Entity Clone() => new StraightContinuationEntity(this);
}