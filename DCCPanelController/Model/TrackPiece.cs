using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model;

public partial class TrackPiece : ObservableObject {

    [ObservableProperty] private TrackTypesEnum _trackType = TrackTypesEnum.None;
    [ObservableProperty] private Color  _color = Colors.Black;
    [ObservableProperty] private string _coordinate = string.Empty;
    [ObservableProperty] private int _rotation = 0;

}