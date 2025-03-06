using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.DataModel.Helpers;
using DCCPanelController.Services;

namespace DCCPanelController.Model.DataModel.Tracks;

public abstract partial class Track : ObservableObject {

    public abstract string Name { get; }
    public virtual string TrackClass => GetType().Name;
    
    [ObservableProperty] private int _x;                    // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                    // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _z = 1;                // What position (layer) should this exist at 
    [ObservableProperty] private int _width  = 1;           // What width is this component?
    [ObservableProperty] private int _height = 1;           // What Height is this component? 
    [ObservableProperty] private int _rotation = 0;         // Is the track rotated?
    [ObservableProperty] private bool _isEnabled = true;    // Is this item actually in use?
    [ObservableProperty] private bool _isSelected = false;  // Is this item actually in use?
    [ObservableProperty] private TrackTypeEnum _trackType = TrackTypeEnum.Normal;
    
    [JsonIgnore] public Panel? Parent { get; set; }
    
    [JsonConstructor]
    protected Track() { }

    /// <summary> 
    /// Use to create a new instance of a Track by making a copy of another track (Clone)
    /// </summary>
    /// <param name="track">A new instance of a Track Object</param>
    protected Track(Track track) {
        X = track.X;
        Y = track.Y;
        Z = track.Z;
        Width = track.Width;
        Height = track.Height;
        Rotation = track.Rotation;
        IsEnabled = track.IsEnabled;
        IsSelected = false;
        TrackType = track.TrackType;
        Parent = track.Parent;
    }
    
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