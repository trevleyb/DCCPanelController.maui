using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.Elements;

[DebuggerDisplay("Type={Type} at {Coordinate},{Width}x{Height}")]
public partial class PanelElement : ObservableObject {

    [ObservableProperty] private string _type = "unknown";
    [ObservableProperty] private Color  _color = Colors.Black;
    [ObservableProperty] private Coordinate _coordinate = new Coordinate();
    [ObservableProperty] private int _rotation = 0;
}

