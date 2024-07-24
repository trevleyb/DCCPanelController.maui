using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class ButtonPanelElement : Base.PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "ButtonPanelElement";
}