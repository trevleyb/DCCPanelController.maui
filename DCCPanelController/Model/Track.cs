using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model;

public partial class Track : ObservableObject {

    [ObservableProperty] private TrackTypesEnum _trackType = TrackTypesEnum.None;
    [ObservableProperty] private Color  _color = Colors.Black;
    [ObservableProperty] private Coordinate _coordinate = new Coordinate();
    [ObservableProperty] private int _width = 1;
    [ObservableProperty] private int _rotation = 0;
}

public enum TrackTypesEnum {
    None, 
    StraightTrack,
    Crossing,
    Terminator,
    LeftTrack,
    RightTrack,
    LeftTurnout,
    RightTurnout,
    WyeJunction
} 