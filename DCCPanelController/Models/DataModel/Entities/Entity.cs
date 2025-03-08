using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;

namespace DCCPanelController.Models.DataModel.Entities;

[DebuggerDisplay("{Name}:{TrackClass}: {Col},{Row}")]
public abstract partial class Entity : ObservableObject {

    public abstract string Name { get; }
    public virtual string TrackClass => GetType().Name;
    
    [ObservableProperty] private int _col;                  // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _row;                  // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _layer = 1;            // What position (layer) should this exist at 
    [ObservableProperty] private int _width  = 1;           // What width is this component?
    [ObservableProperty] private int _height = 1;           // What Height is this component? 
    [ObservableProperty] private int _rotation = 0;         // Is the track rotated?
    [ObservableProperty] private bool _isEnabled = true;    // Is this item actually in use?
    [ObservableProperty] private bool _isSelected = false;  // Is this item actually in use?
    [ObservableProperty] private TrackTypeEnum _trackType = TrackTypeEnum.Normal;
    
    [JsonIgnore] public Panel? Parent { get; set; }
    [JsonIgnore] public Guid Guid { get; init; } = Guid.NewGuid(); 

    [JsonConstructor]
    protected Entity() { Parent = null; }
    protected Entity(Panel panel) : this() { 
        Parent = panel;
    }
    protected Entity(Entity entity) : this() {
        Col = entity.Col;
        Row = entity.Row;
        Layer = entity.Layer;
        Width = entity.Width;
        Height = entity.Height;
        Rotation = entity.Rotation;
        IsEnabled = entity.IsEnabled;
        IsSelected = false;
        TrackType = entity.TrackType;
    }
    public abstract Entity Clone();
}
 
public enum ButtonStateEnum { Unknown, On, Off  }
public enum TurnoutStateEnum { Unknown, Closed, Thrown }
public enum RouteStateEnum { Unknown, Active, Inactive, }
public enum TerminatorStyleEnum { Unknown, Normal, Arrow, Lines}

[Flags]
public enum TrackTypeEnum {
    Normal          = 0,                    // 00000000 
    Mainline        = 1,                    // 00000001
    BranchLine      = 2,                    // 00000010
    Hidden          = 4,                    // 00000100
    MainlineHidden  = Mainline | Hidden,    // 00000101
    BranchlineHidden = BranchLine | Hidden  // 00000110
}