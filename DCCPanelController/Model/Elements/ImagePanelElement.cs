namespace DCCPanelController.Model.Elements;

public partial class ImagePanelElement : PanelElement, IPanelElement {
    public string ElementType => GetType()?.Name ?? "ImagePanelElement";
}