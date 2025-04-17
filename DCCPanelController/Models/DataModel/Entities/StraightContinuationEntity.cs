using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class StraightContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableTrackTerminator("Terminator", group: "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

    [JsonConstructor]
    public StraightContinuationEntity() { }

    public StraightContinuationEntity(Panel panel) : this() {
        Parent = panel;
    }

    public StraightContinuationEntity(StraightContinuationEntity entity) : base(entity) {
        ContinuationStyle = entity.ContinuationStyle;
    }

    public override string EntityName => "Straight Track";

    public override Entity Clone() {
        return new StraightContinuationEntity(this);
    }
}