using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class RoutePanelElement : Base.PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "RoutePanelElement";
}