using System.Reflection;
using System.Text;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace DCCPanelController.Components.SVGManager;

public class SvgImageManager {

    private const int DefaultWidth = 192;
    private const int DefaultHeight = 192;
    private readonly XDocument _svgImageXDoc;

    /// <summary>
    /// Creates an instance of the Image Manager with the given name of the
    /// image to manage. This needs to be a part of the resource name as it will
    /// find the first match in the list of .SVG resources that matches the name.
    /// If it does not find one, it will throw a FileNotFound exception. 
    /// </summary>
    /// <param name="imageName">The name of the image such as Track_Angle.svg. Can insensitive</param>
    /// <exception cref="Exception">Will throw FileNot Found if it cannot find the file. </exception>
    public SvgImageManager(string imageName) {
        try {
            _svgImageXDoc = LoadSvg(SvgImageFinder.GetFullPathOfResource(imageName));
        } catch (Exception e) {
            throw new FileNotFoundException($"Unable to load image {imageName}: {e.Message}");
        }
    }

    public ImageSource Image => GetSvgAsImage();

    /// <summary>
    /// Converts the SVG Impage into a PNG. Upscales it to the default size as part of the process. 
    /// </summary>
    /// <returns>A PNG Image of the SVG</returns>
    /// <exception cref="ApplicationException"></exception>
    private ImageSource GetSvgAsImage() {
        var svg = new SKSvg();
        svg.Load(new MemoryStream(Encoding.UTF8.GetBytes(_svgImageXDoc.ToString())));
        if (svg is null) throw new ApplicationException("Unable to load svg");

        const int quality = 100;
        var scaleX = DefaultWidth / svg.Picture?.CullRect.Width ?? DefaultWidth;
        var scaleY = DefaultHeight / svg.Picture?.CullRect.Height ?? DefaultHeight;

        var stream = new MemoryStream();
        svg.Save(stream, SKColor.Empty, SKEncodedImageFormat.Png, quality, scaleX, scaleY);
        stream.Seek(0, SeekOrigin.Begin);
        return ImageSource.FromStream(() => stream);
    }

    /// <summary>
    /// Converts the XDocument SVG Image into a stream which can be consumed by
    /// the ImageSource.FromStream method
    /// </summary>
    private static Stream ConvertXDocumentToStream(XDocument svgDocument) {
        var stream = new MemoryStream();
        svgDocument.Save(stream);
        stream.Position = 0;
        return stream;
    }

    private static XDocument LoadSvg(string resourceName) {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException("Resource not found.", resourceName);

        using var reader = new StreamReader(stream);
        var svgContent = reader.ReadToEnd();
        try {
            var xDocument = XDocument.Parse(svgContent);
            return xDocument;
        } catch (Exception ex) {
            throw new FileLoadException("Failed to load the SVG image.", ex);
        }
    }

    #region Manage Changing Colors and Opacity using the attribute for the tem to change
    protected List<XElement> FindElements(string attributeID) => FindElementsAttribute("id", attributeID).ToList();

    protected List<XElement> FindElementsAttribute(string attributeName, string attributeValue) {
        ArgumentNullException.ThrowIfNull(_svgImageXDoc);
        var elements = new List<XElement>();
        foreach (var element in _svgImageXDoc.Descendants()) {
            foreach (var attr in element.Attributes()) {
                if (attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) && attr.Value.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) {
                    elements.Add(element);
                }
            }
        }

        return elements;
    }

    public bool IsElementSupported(string name) => SupportedElements.Contains(name, StringComparer.OrdinalIgnoreCase);
    public List<string> SupportedElements => _svgImageXDoc.Descendants()
        .SelectMany(element => element.Attributes()
        .Where(attribute => attribute.Name.LocalName == "id")
        .Select(attribute => attribute.Value))
        .Distinct().ToList();
    
    /// <summary>
    /// Sets the value of a given Attribute within a given Element
    /// </summary>
    /// <param name="elementName">The element to find with an ID = to the element name</param>
    /// <param name="attributeName">The attribute name to modify</param>
    /// <param name="attributeValue">The value to apply</param>
    public void SetElementAttributeValue(string elementName, string attributeName, string attributeValue) {
        foreach (var element in FindElements(elementName)) {
            SetAttributeValue(element, attributeName, attributeValue);
        }
    }

    /// <summary>
    /// Sets the Element value for an element that has the given ID
    /// </summary>
    /// <param name="elementName">The element that has id='<elementname>'</param>
    /// <param name="elementValue"The value to populate the element value with></param>
    public void SetElementValue(string elementName, string elementValue) {
        foreach (var element in FindElements(elementName)) {
            element.Value = elementValue;
        }
    }
    
    public string GetElementType(string elementName) {
        var element = FindElements(elementName).FirstOrDefault();
        return element?.Name?.LocalName ?? "unknown";
    }
    
    protected static void SetAttributeValue(XElement element, string attributeName, string attributeValue) {
        ArgumentNullException.ThrowIfNull(element);
        var attribute = (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr).FirstOrDefault();
        if (attribute is not null) {
            attribute.Value = attributeValue;
        } else {
            element.Add(new XAttribute(attributeName, attributeValue));
        }
    }

    protected static string? GetAttributeValue(XElement element, string attributeName) {
        ArgumentNullException.ThrowIfNull(element);
        return (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr.Value).FirstOrDefault();
    }
    #endregion
}
