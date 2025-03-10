using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;
using SkiaSharp;
using SKSvg = Svg.Skia.SKSvg;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public class SvgImageManager {
    private const int DefaultWidth = 192;
    private const int DefaultHeight = 192;
    private readonly XDocument _svgDocument;
    private string? _resourceName;

    /// <summary>
    ///     Creates an instance of the DisplayImage Manager with the given name of the
    ///     image to manage. This needs to be a part of the resource name as it will
    ///     find the first match in the list of .SVG resources that matches the name.
    ///     If it does not find one, it will throw a FileNotFound exception.
    /// </summary>
    /// <param name="imageName">The name of the image such as Track_Angle.svg. Can insensitive</param>
    /// <exception cref="Exception">Will throw FileNot Found if it cannot find the file. </exception>
    public SvgImageManager(string imageName) {
        try {
            _svgDocument = LoadSvg(imageName);
        } catch (Exception e) {
            throw new FileNotFoundException($"Unable to load image {imageName}: {e.Message}");
        }
    }

    /// <summary>
    ///     Returns the image. We store it so that we only need to re-calculate it if we have made a
    ///     change to any of the elements. The change functions will set _imageSource to null which will
    ///     cause a call to the image function to re-calculate/re-draw the image itself.
    /// </summary>
    public ImageSource ImageSource => GetSvgAsImage();
    public List<string> SupportedElements => _svgDocument.Descendants().SelectMany(element => element.Attributes().Where(attribute => attribute.Name.LocalName == "id").Select(attribute => attribute.Value)).Distinct().ToList();
    
    /// <summary>
    ///     Converts the SVG DisplayImage into a PNG. Up-scales it to the default size as part of the process.
    /// </summary>
    /// <returns>A PNG DisplayImage of the SVG</returns>
    private ImageSource GetSvgAsImage() {
        var svg = new SKSvg();
        svg.Load(new MemoryStream(Encoding.UTF8.GetBytes(_svgDocument.ToString())));
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
    ///     Converts the XDocument SVG DisplayImage into a stream which can be consumed by
    ///     the ImageSource.FromStream method
    /// </summary>
    private Stream ConvertXDocumentToStream(XDocument svgDocument) {
        var stream = new MemoryStream();
        svgDocument.Save(stream);
        stream.Position = 0;
        return stream;
    }

    private XDocument LoadSvg(string resourceName) {
        _resourceName = resourceName;
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(_resourceName);

        if (stream == null) {
            Debug.WriteLine($"Could not find the image resource: '{_resourceName}'");
            throw new FileNotFoundException("Resource not found.", _resourceName);
        }

        using var reader = new StreamReader(stream);
        var svgContent = reader.ReadToEnd();

        try {
            var xDocument = XDocument.Parse(svgContent);
            return xDocument;
        } catch (Exception ex) {
            Debug.WriteLine($"Failed to load the SVG image: '{_resourceName}' with {ex.Message}");
            throw new FileLoadException("Failed to load the SVG image.", ex);
        }
    }

    /// <summary>
    ///     Forces a set of any attributes defined in the element to the value. Does not add the attribute if it does not exist
    /// </summary>
    public void SetAllAttributeValues(SvgElementType svgElement, string attributeName, string attributeValue) => SetAllAttributeValues(SvgElementTypes.GetElement(svgElement), attributeName, attributeValue);
    public void SetAllAttributeValues(string svgElement, string attributeName, string attributeValue) {
        foreach (var element in FindElements(svgElement)) {
            SetAttributeValue(element, attributeName, attributeValue, false);
        }
    }

    /// <summary>
    ///     This will search through the document and find all elements where the id= the name of the element to find.
    ///     As an example, give the following XML for a Button:
    ///     <circle id="Border" stroke="#000000" stroke-width="2" fill="#FFFFFF" cx="24" cy="24" r="7"></circle>
    ///     Then a search for 'Border' will match on this ID, and the 'circle' element will be returned.
    /// </summary>
    public List<XElement> FindElements(string elementName) {
        return (from element in _svgDocument.Descendants() from attr in element.Attributes() where attr.Name.LocalName.Equals("id", StringComparison.OrdinalIgnoreCase) && attr.Value.Equals(elementName, StringComparison.OrdinalIgnoreCase) select element).ToList();
    }

    public string ElementType(XElement element) {
        return element.Name.LocalName.ToLowerInvariant();
    }

    public bool IsElementOfType(XElement element, string type) {
        return element.Name.LocalName.Equals(type, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Given an element, set the attribute property to the value provided
    /// </summary>
    public void SetAttributeValue(XElement element, string attributeName, string attributeValue, bool addIfNotExist = true) {
        ArgumentNullException.ThrowIfNull(element);
        var attribute = (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr).FirstOrDefault();

        if (attribute is not null) {
            attribute.Value = attributeValue;
        } else {
            if (addIfNotExist) {
                element.Add(new XAttribute(attributeName, attributeValue));
            }
        }
    }

    public void SetElementValue(XElement element, string attributeValue) {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(attributeValue);
    }

    /// <summary>
    ///     Get the value of an attribute given an element
    /// </summary>
    private static string? GetAttributeValue(XElement element, string attributeName) {
        ArgumentNullException.ThrowIfNull(element);
        return (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr.Value).FirstOrDefault();
    }
}