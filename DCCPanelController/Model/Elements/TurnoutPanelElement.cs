using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class TurnoutPanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "TurnoutpanelElement";
}