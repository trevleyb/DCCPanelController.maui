using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.Tracks;
using DCCPanelController.Components.Tracks.SVGManager;
using Microsoft.Maui.Handlers;
using ShimSkiaSharp;

namespace DCCPanelController.Components.TrackImages;

public partial class TrackImage : ObservableObject {

    [ObservableProperty] private string _name;
    [ObservableProperty] private int _rotation;

    [ObservableProperty] private SvgImageManager _imageManager;
    [ObservableProperty] private TrackConnections _connections;
    
    public TrackImage(string name, string imageName, int rotation, TrackConnections connections) {
        Name = name;
        Rotation = rotation;
        Connections = connections;
        ImageManager = new SvgImageManager(imageName);
    }

    public ImageSource? Image => ImageManager.Image;

    public void ApplyStyle(string styleName) {
        TrackStyles.ApplyStyle(styleName, this);
    }

    public void ApplyStyle(TrackStyle style) {
        foreach (var element in style.StyleElements) {
            foreach (var attribute in element.Attributes) {
                SetElementAttribute(element.Name, attribute.Name, attribute.Value);
            }
        }
    }
    
    public void SetElementAttribute(string elementName, string attributeName, string attributeValue) {
        switch (attributeName) {
        case "Color":
            if (ImageManager.GetElementType(elementName).Equals("line", StringComparison.OrdinalIgnoreCase)) {
                ImageManager.SetElementValue(elementName, "stroke", attributeValue);
            } else {
                ImageManager.SetElementValue(elementName, "fill", attributeValue);
            }
            break;
        case "Opacity":
            ImageManager.SetElementValue(elementName, "opacity", attributeValue);
            break;
        case "Dashed":
            if (ImageManager.GetElementType(elementName).Equals("line", StringComparison.OrdinalIgnoreCase)) {
                if (attributeValue.Equals("1") || attributeValue.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                    ImageManager.SetElementValue(elementName, "stroke-dasharray", "2,6");
                    ImageManager.SetElementValue(elementName, "stroke-linecap", "square");
                } else {
                    ImageManager.SetElementValue(elementName, "stroke-dasharray", "0,0");
                }
            }
            break;
        }
    }
    
}
