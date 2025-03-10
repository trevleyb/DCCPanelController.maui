using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;
public partial class CornerContinuationEntity : TrackEntity, ITrackEntity {
    public override string Name => "Corner Track";
    [ObservableProperty] private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;
    
    [JsonConstructor]
    public CornerContinuationEntity() {}
    public CornerContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }
    public CornerContinuationEntity(CornerContinuationEntity entity) : base(entity) {
        ContinuationStyle = entity.ContinuationStyle;
    }
    public override Entity Clone() => new CornerContinuationEntity(this);
}