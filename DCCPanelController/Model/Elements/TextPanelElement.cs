using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class TextPanelElement : PanelElement, IPanelElement {
    [ObservableProperty] private string _text = "This is some text";
    [ObservableProperty] private string _font = "";
    [ObservableProperty] private int _fontSize = 12;
    [ObservableProperty] private Color _fontColor = Colors.Black;
    [ObservableProperty] private Color _backgroundColor = Colors.Transparent;

    public string ElementType => GetType()?.Name ?? "TextPanelElement";
}