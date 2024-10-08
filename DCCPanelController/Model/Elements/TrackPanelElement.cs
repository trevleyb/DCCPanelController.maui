using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class TrackPanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "TrackPanelElement";
}