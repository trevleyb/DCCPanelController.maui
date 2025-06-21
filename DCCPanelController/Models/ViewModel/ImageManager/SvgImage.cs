using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public partial class SvgImage : ObservableObject {
    [ObservableProperty] private string _filename = string.Empty;
    [ObservableProperty] private int _rotation;

    public SvgImage(string filename, int rotation) {
        Filename = filename;
        Rotation = rotation;
        ImageManager = new SvgImageManager(filename);
    }

    [field: AllowNull, MaybeNull] 
    private SvgImageManager ImageManager => field ??= new SvgImageManager(Filename);

    public ImageSource AsImageSource(int rotation = 0, float scale = 1.0f) {
        return ImageManager.AsImageSource(rotation, scale);
    }

    public Image AsImage(int rotation = 0, float scale = 1.0f) {
        return new Image { Source = ImageManager.AsImageSource(rotation, scale), HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
    }

    public SKCanvasView AsCanvas(int rotation = 0, float scale = 1.5f) {
        return ImageManager.AsCanvasView(rotation, scale);
    }

    public void SetAttribute(SvgElementType elementType, Color color) {
        ImageManager.SetAllAttributeValues(elementType, "fill", color.ToHex());
    }

    public void SetLabel(string label) {
        if (string.IsNullOrEmpty(label)) return;
        ImageManager.SetAllAttributeValues(SvgElementType.Text, "text", label);
    }

    public SvgImage ApplyStyle(SvgStyle style) {
        foreach (var element in style.Elements) {
           foreach (var styleAttribute in element.Value.Attributes) {
                ApplyElementStyle(element.Key, styleAttribute.Key, styleAttribute.Value);
           }
        }
        return this;
    }

    // Element Types Supported Include
    // ---------------------------------------------------------------------------
    // <rect>       - fill, fill-opacity
    // <polygon>    - fill, fill-opacity
    // <text>       - fill, fill-opacity
    // <line>       - stroke, stroke-opacity, dash-array
    // <circle>     - Border= stroke, stroke-opacity, Button=fill, fill-opacity 

    // Supported EditableAttribute for Styles
    // ---------------------------------------------------------------------------
    // Color    - sets the color of the element
    // Opacity  - sets the opacity of the element
    // Dashed   - sets if the line is dashed or not dashed (valid on a line only)
    // Visible  - sets if the element should be visible or not
    // Text     - sets the TEXT of the item if it has a Text Element

    public void ApplyElementStyle(SvgElementType svgElement, string attributeName, string value) {
        ApplyElementStyle(SvgElementTypes.GetElement(svgElement), attributeName, value);
    }

    public void ApplyElementStyle(string elementName, string attributeName, string attributeValue) {
        // Get back all the elements that have an ID = the element name provided (such as "border")
        // -----------------------------------------------------------------------------------------
        var elements = ImageManager.FindElements(elementName);
        foreach (var element in elements) {
            var elementType = ImageManager.ElementType(element);
            //Console.WriteLine($"Applying {attributeName} = {attributeValue} to {elementName} with type {elementType}");

            _ = elementType switch {
                "rect"    => SetFillType(element, attributeName, attributeValue),
                "polygon" => SetFillType(element, attributeName, attributeValue),
                "text" => elementName.ToLowerInvariant() switch {
                    "text"  => SetTextData(element, attributeName, attributeValue),
                    "color" => SetFillType(element, attributeName, attributeValue),
                    _       => SetFillType(element, attributeName, attributeValue)
                },
                "line" => SetStrokeType(element, attributeName, attributeValue),
                "circle" => elementName.ToLowerInvariant() switch {
                    "border" => SetStrokeType(element, attributeName, attributeValue),
                    "button" => SetFillType(element, attributeName, attributeValue),
                    _        => SetFillType(element, attributeName, attributeValue)
                },
                _ => SetFillType(element, attributeName, attributeValue)
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

    private bool SetTextData(XElement element, string attributeName, string attributeValue) {
        switch (attributeName.ToLowerInvariant()) {
        case "text":
            ImageManager.SetElementValue(element, attributeValue);
            break;

        case "font-size":
            ImageManager.SetAttributeValue(element, "font-size", attributeValue);
            break;

        case "font-weight":
            ImageManager.SetAttributeValue(element, "font-weight", attributeValue);
            break;

        case "font-color":
            ImageManager.SetAttributeValue(element, "fill", attributeValue);
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