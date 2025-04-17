using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.DynamicProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    [ObservableProperty] [property: EditableOpacity("Opacity", "", 5, "Track")]
    private double _opacity = 1.0;

    [ObservableProperty] [property: EditableTrack("Attributes", "", 5, "Track")]
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, "Track")]
    private Color? _trackBorderColor;

    [ObservableProperty] [property: EditableColor("Track Color", "", 5, "Track")]
    private Color? _trackColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainLine))]
    [property: EditableTrackType("Track Type", "", 5, "Track")]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;

    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
        Rotation = entity.Rotation;
    }

    public bool IsMainLine => TrackType == TrackTypeEnum.MainLine;
}