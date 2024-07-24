using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class TrackPanelElement : Base.PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "TrackPanelElement";
}