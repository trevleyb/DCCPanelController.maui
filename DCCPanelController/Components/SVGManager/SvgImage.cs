using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Components.SVGManager;

[DebuggerDisplay("{Id} [{Rotation}]")]
public partial class SvgImage : ObservableObject {

    [ObservableProperty] private string _id;
    [ObservableProperty] private int _rotation;

    [ObservableProperty] private SvgImageManager _imageManager;
    [ObservableProperty] private SvgCompass _connections;
    
    public SvgImage(string id, string imageName, int rotation, SvgCompass connections) {
        Id = id;
        Rotation = rotation;
        Connections = connections;
        ImageManager = new SvgImageManager(imageName);
    }

    public ImageSource? Image => ImageManager.Image;

    public void ApplyStyle(string styleName) {
        SvgStyles.ApplyStyle(styleName, this);
    }

    public bool SupportsLabel => ImageManager.IsElementSupported("text");
    public void SetLabel(string label) {
        if (string.IsNullOrEmpty(label)) return;
        if (ImageManager.IsElementSupported("text")) ImageManager.SetElementValue("text", label);
    }

    public void ApplyStyle(SvgStyle style) {
        foreach (var element in style.Elements) {
            foreach (var attribute in element.Attributes) {
                SetElementAttribute(element.Name, attribute.Name, attribute.Value);
            }
        }
    }
    
    public void SetElementAttribute(string elementName, string attributeName, string attributeValue) {
        switch (attributeName) {
        case "Color":
            if (ImageManager.GetElementType(elementName).Equals("line", StringComparison.OrdinalIgnoreCase)) {
                ImageManager.SetElementAttributeValue(elementName, "stroke", attributeValue);
            } else {
                ImageManager.SetElementAttributeValue(elementName, "fill", attributeValue);
            }
            break;
        case "Opacity":
            ImageManager.SetElementAttributeValue(elementName, "opacity", attributeValue);
            break;
        case "Text":
            if (ImageManager.GetElementType(elementName).Equals("text", StringComparison.OrdinalIgnoreCase)) {
                ImageManager.SetElementValue(elementName, attributeValue);
            }
            break;

        case "Dashed":
            if (ImageManager.GetElementType(elementName).Equals("line", StringComparison.OrdinalIgnoreCase)) {
                if (attributeValue.Equals("1") || attributeValue.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    ImageManager.SetElementAttributeValue(elementName, "stroke-dasharray", "2,6");
                    ImageManager.SetElementAttributeValue(elementName, "stroke-linecap", "square");
                } else {
                    ImageManager.SetElementAttributeValue(elementName, "stroke-dasharray", "0,0");
                }
            }
            break;
        }
    }
    
}
