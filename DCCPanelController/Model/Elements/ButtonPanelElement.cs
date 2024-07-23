using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.Elements;

public partial class ButtonPanelElement : PanelElement, IPanelElement {
    
    [ObservableProperty] private string _text = "This is some text";
    [ObservableProperty] private string _font = "";
    [ObservableProperty] private int _fontSize = 12;
    
    
    public string ElementType => GetType()?.Name ?? "ButtonPanelElement";
}