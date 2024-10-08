using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class CircleTextPanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "CircleTextPanelElement";
    
    [ObservableProperty] private string _text = "This is some text";
    [ObservableProperty] private string _font = "";
    [ObservableProperty] private int _fontSize = 12;
    [ObservableProperty] private Color _fontColor = Colors.Black;

    
}