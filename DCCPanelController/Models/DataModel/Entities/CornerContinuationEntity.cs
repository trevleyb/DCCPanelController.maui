using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.DynamicProperties.EditableControls;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class CornerContinuationEntity : TrackEntity, ITrackEntity {
    [ObservableProperty] [property: EditableTrackTerminator("Terminator", group: "Track")]
    private TrackTerminatorEnum _continuationStyle = TrackTerminatorEnum.Arrow;

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