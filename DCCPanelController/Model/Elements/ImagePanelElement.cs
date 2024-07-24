using DCCPanelController.Model.Elements.Base;

namespace DCCPanelController.Model.Elements;

public partial class ImagePanelElement : Base.PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "ImagePanelElement";
}