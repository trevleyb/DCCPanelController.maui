using CommunityToolkit.Mvvm.ComponentModel;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    
    [ObservableProperty] private Color? _trackColor;
    [ObservableProperty] private Color? _trackBorderColor;
    [ObservableProperty] private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;
    [ObservableProperty] private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
    }
}