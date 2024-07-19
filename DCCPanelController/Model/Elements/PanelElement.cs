using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.Elements;

[DebuggerDisplay("Type={Type} at {Coordinate}")]
public partial class PanelElement : ObservableObject {

    [ObservableProperty] private string _type = "unknown";
    [ObservableProperty] private Color  _color = Colors.Black;
    [ObservableProperty] private Coordinate _coordinate = new Coordinate();
    [ObservableProperty] private int _width = 1;
    [ObservableProperty] private int _height = 1;
    [ObservableProperty] private int _rotation = 0;
}

