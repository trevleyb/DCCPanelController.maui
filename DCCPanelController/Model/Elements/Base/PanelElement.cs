using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.Elements.Base;

[DebuggerDisplay("Type={Type} at {Coordinate}")]
public abstract partial class PanelElement : ObservableObject {

    [ObservableProperty] private string _symbolType = "set:unknown";
    [ObservableProperty] private Coordinate _coordinate = new Coordinate();
    [ObservableProperty] private Color  _color = Colors.Black;
    [ObservableProperty] private int _rotation = 0;
}

