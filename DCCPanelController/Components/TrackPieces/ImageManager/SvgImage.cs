using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;

namespace DCCPanelController.Components.TrackPieces.SVGManager;

[DebuggerDisplay("{Id} [{Rotation}]")]
public partial class SvgImage : ObservableObject {

    [ObservableProperty] private string _id;
    [ObservableProperty] private int _rotation;
    [ObservableProperty] private bool _isOccupied;
    [ObservableProperty] private SvgImageManager _imageManager;
    [ObservableProperty] private SvgCompass _connections;

    public ImageSource? Image => ImageManager.Image;
    public void ForceImageRefresh() => ImageManager.ForceImageRefresh();

    public SvgImage(string id, string imageName, int rotation, SvgCompass connections) {
        Id = id;
        Rotation = rotation;
        Connections = connections;
        ImageManager = new SvgImageManager(imageName);
        IsOccupied = false;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName is nameof(IsOccupied)) {
            ApplyStyle(IsOccupied ? "track-occupied" : "track-free");
        } 
    }

    public bool SupportsLabel => ImageManager.IsSupported(SvgElementEnum.Text);
    public void SetLabel(string label) {
        if (string.IsNullOrEmpty(label)) return;
        if (SupportsLabel) ImageManager.SetAllAttributeValues(SvgElementEnum.Text, "text", label);
    }

    public void SetOccupied(bool isOccupied) {
        IsOccupied = isOccupied;
        ApplyStyle(IsOccupied ? "track-occupied" : "track-free");
    }
    
    public void ApplyStyle(string style) => ApplyStyle(SvgStyles.GetStyle(style));
    
    public void ApplyStyle(SvgStyle style) {
        if (string.IsNullOrEmpty(style.Name)) return; 
        foreach (var element in style.Elements) {
            foreach (var styleAttribute in element.Value.Attributes) {
                ApplyElementStyle(element.Key, styleAttribute.Key, styleAttribute.Value);
            }
        }
    }
    
    // Element Types Supported Include
    // ---------------------------------------------------------------------------
    // <rect>       - fill, fill-opacity
    // <polygon>    - fill, fill-opacity
    // <text>       - fill, fill-opacity
    // <line>       - stroke, stroke-opacity, dash-array
    // <circle>     - Border= stroke, stroke-opacity, Button=fill, fill-opacity 
    
    // Supported Attributes for Styles
    // ---------------------------------------------------------------------------
    // Color        - sets the color of the element
    // Opacity      - sets the opacity of the element
    // Dashed       - sets if the line is dashed or not dashed (valid on a line only)
    // Visible      - sets if the element should be visible or not 
    
    public void ApplyElementStyle(SvgElementEnum elementEnum, string attributeName, string value) => ApplyElementStyle(SvgElement.ToString(elementEnum), attributeName, value);
    public void ApplyElementStyle(string elementName, string attributeName, string attributeValue) {
        // Get back all the elements that have an ID = the element name provided (such as "border")
        // -----------------------------------------------------------------------------------------
        foreach (var element in ImageManager.FindElements(elementName)) {
            _ = ImageManager.ElementType(element) switch {
                "rect"    => SetFillType(element, attributeName, attributeValue),
                "polygon" => SetFillType(element, attributeName, attributeValue),
                "text"    => SetFillType(element, attributeName, attributeValue),
                "line"    => SetStrokeType(element, attributeName, attributeValue),
                "circle" => elementName.ToLowerInvariant() switch {
                    "border" => SetStrokeType(element, attributeName, attributeValue),
                    "button" => SetFillType(element, attributeName, attributeValue),
                    _ => false
                },
                _ => false,
            };
        }
    }
    
    private bool SetFillType(XElement element, string attributeName, string attributeValue) {
        switch (attributeName.ToLowerInvariant()) {
        case "color":
            ImageManager.SetAttributeValue(element, "fill", attributeValue);
            break;
        case "opacity":
            ImageManager.SetAttributeValue(element, "fill-opacity", attributeValue);
            break;
        case "visible":
            ImageManager.SetAttributeValue(element, "fill-opacity", attributeValue.IsTrue() ? "100" : "0");
            break;
        default:
            return false;
        }
        return true;
    }

    private bool SetStrokeType(XElement element, string attributeName, string attributeValue) {
        switch (attributeName.ToLowerInvariant()) {
        case "color":
            ImageManager.SetAttributeValue(element, "stroke", attributeValue);
            break;
        case "opacity":
            ImageManager.SetAttributeValue(element, "stroke-opacity", attributeValue);
            break;
        case "dashed":
            ImageManager.SetAttributeValue(element, "stroke-dasharray", attributeValue.IsTrue() ? "2,6" : "0,0");
            break;
        case "visible":
            ImageManager.SetAttributeValue(element, "stroke-opacity", attributeValue.IsTrue() ? "100" : "0");
            break;
        default:
            return false;
        }
        return true;
    }
}
