using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.DynamicProperties;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainLine))]
    [property: EditableTrackType("Track Type", "", 5, group: "Track")]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;
    
    [ObservableProperty][property: EditableTrackAttribute("Attributes", "", 5, group: "Track")] 
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    [ObservableProperty] [property: EditableColor("Track Color", "", 5, group: "Track")]
    private Color? _trackColor;
    
    [ObservableProperty] [property: EditableColor("Border Color", "", 5, group: "Track")]
    private Color? _trackBorderColor;
    
    [ObservableProperty] [property: EditableOpacity("Opacity", "", 5, group: "Track" )]
    private double _opacity = 1.0;

    public bool IsMainLine => TrackType == TrackTypeEnum.MainLine;
    
    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
        Rotation = entity.Rotation;
    }
}