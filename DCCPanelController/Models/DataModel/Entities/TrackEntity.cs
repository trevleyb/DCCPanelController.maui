using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    [ObservableProperty] [property: EditableBlock("Occupancy Block", "", 1, "Track")]
    private string? _occupancyBlock;

    [ObservableProperty] [property: EditableEnum("Attributes", "", 5, "Track")]
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, "Track")]
    private Color? _trackBorderColor;

    [ObservableProperty] [property: EditableColor("Track Color", "", 5, "Track")]
    private Color? _trackColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainLine))]
    [property: EditableEnum("Track Type", "", 5, "Track")]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;

    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    [JsonIgnore] public Block? Occupancy {
        get {
            if (string.IsNullOrEmpty(OccupancyBlock)) return null;
            return Parent?.Block(OccupancyBlock) ?? null;
        }
    } 
    
    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
        Rotation = entity.Rotation;
    }

    public bool IsMainLine => TrackType == TrackTypeEnum.MainLine;
}