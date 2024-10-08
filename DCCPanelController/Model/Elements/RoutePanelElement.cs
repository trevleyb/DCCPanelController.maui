using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class RoutePanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "RoutePanelElement";
}