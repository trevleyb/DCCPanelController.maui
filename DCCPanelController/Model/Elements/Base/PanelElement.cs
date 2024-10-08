using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;

namespace DCCPanelController.Model.Elements.Base;

[DebuggerDisplay("Type={SymbolType} at {Coordinate}")]
public abstract partial class PanelElement : ObservableObject {

    [ObservableProperty] private string _symbolType = "set:unknown";
    [ObservableProperty] private Coordinate _coordinate = new Coordinate();
    [ObservableProperty] private int _rotation = 0;
    
    [ObservableProperty]
    [property: EditableProperty(Name = "Default Color")]
    private Color  _color = Colors.Black;
}

