using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model.Elements;

public partial class ButtonPanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "ButtonPanelElement";
}