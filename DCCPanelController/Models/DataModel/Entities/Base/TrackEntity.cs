using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    
    [ObservableProperty] [property: Editable("Track Color", EditableType.Color)]
    private Color? _trackColor;
    
    [ObservableProperty] [property: Editable("Border Color", EditableType.Color)]
    private Color? _trackBorderColor;
    
    [ObservableProperty] [property: Editable("Track Type", EditableType.TrackType)]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;
    
    [ObservableProperty][property: Editable("Attributes", EditableType.TrackAttributes)] 
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
    }
}