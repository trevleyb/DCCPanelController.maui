using System.Reflection;
using System.Text;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace DCCPanelController.Components.Tracks.SVGManager;

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
        svg.Save(stream,SKColor.Empty, SKEncodedImageFormat.Png, quality, scaleX, scaleY);
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

    public void SetElementColor(string elementName, Color? color, int? opacity) {
        foreach (var element in FindElements(elementName)) {
            if (color is not null) SetAttributeValue(element, "fill", color.ToHex());
            if (opacity is >= 0 and <= 100) SetAttributeValue(element, "fill-opacity", opacity.ToString() ?? "100");
        }
    }
    
    #region Manage Changing Colors and Opacity using the attribute for the tem to change
    protected List<XElement> FindElements(string elementName) => FindElementsAttribute("", "id", elementName).ToList();
    protected List<XElement> FindElementsAttribute(string elementName, string attributeName, string attributeValue) {
        ArgumentNullException.ThrowIfNull(_svgImageXDoc);
        var elements = new List<XElement>();
        foreach (var element in _svgImageXDoc.Descendants()) {
            if (string.IsNullOrEmpty(elementName) || element.Name.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase)) {
                foreach (var attr in element.Attributes()) {
                    if (attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) && attr.Value.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) {
                        elements.Add(element);
                    }
                }
            }
        }
        return elements;
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
