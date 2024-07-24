using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class TurnoutPanelElement : Base.PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "TurnoutpanelElement";
}