using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    
    [ObservableProperty] [property: Editable("Track Type", EditableType.TrackType, 5, group: "Track")]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;
    
    [ObservableProperty][property: Editable("Attributes", EditableType.TrackAttributes, 5, group: "Track")] 
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    [ObservableProperty] [property: Editable("Track Color", EditableType.Color, 5, group: "Track")]
    private Color? _trackColor;
    
    [ObservableProperty] [property: Editable("Border Color", EditableType.Color, 5, group: "Track")]
    private Color? _trackBorderColor;
    
    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
    }
}