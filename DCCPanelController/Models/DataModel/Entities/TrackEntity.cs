using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public abstract partial class TrackEntity : Entity {
    [ObservableProperty] [property: EditableBlock("Occupancy Block", "Indicates what block this track is in for showing Occupancy.", 8, "Track")]
    private string? _occupancyBlock;

    [ObservableProperty] [property: EditableEnum("Track Style", "Dashed normally used for hidden track.", 4, "Track")]
    private TrackAttributeEnum _trackAttribute = TrackAttributeEnum.Normal;

    [ObservableProperty] [property: EditableColor("Border Color", "", 5, "Track")]
    private Color? _trackBorderColor;

    [ObservableProperty] [property: EditableColor("Track Color", "", 5, "Track")]
    private Color? _trackColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainLine))]
    [property: EditableEnum("Track Type", "Mainline track has a border. Branch line does not.", 3, "Track")]
    private TrackTypeEnum _trackType = TrackTypeEnum.MainLine;

    protected TrackEntity() { }
    protected TrackEntity(Panel panel) : base(panel) { }

    [JsonIgnore] public Block? Occupancy {
        get {
            if (string.IsNullOrEmpty(OccupancyBlock)) return null;
            return Parent?.Block(OccupancyBlock) ?? null;
        }
    } 
    
    [JsonIgnore] public bool IsMainLine => TrackType == TrackTypeEnum.MainLine;
    [JsonIgnore] public abstract EntityConnections Connections { get; }
    [JsonIgnore] public string RotatedConnections => Connections.GetRotatedConnectionsStr(Rotation);
    [JsonIgnore] public ConnectionType[] GetCurrentConnections => Connections.GetConnections(Rotation);
    [JsonIgnore] public List<int> GetValidDirections => Connections.GetValidDirections(Rotation);
    
    public ConnectionType GetConnection(int direction) => Connections.GetConnection(direction, Rotation);
    protected TrackEntity(TrackEntity entity) : base(entity) {
        TrackColor = entity.TrackColor;
        TrackBorderColor = entity.TrackBorderColor;
        TrackType = entity.TrackType;
        TrackAttribute = entity.TrackAttribute;
        Rotation = entity.Rotation;
    }
}