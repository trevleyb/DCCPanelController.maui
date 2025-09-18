using System.Text;
using System.Xml.Linq;
using DCCPanelController.Models.ViewModel.StyleManager;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Svg.Skia;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public partial class SvgImageManager {
    private const    int       DefaultWidth  = 48;
    private const    int       DefaultHeight = 48;
    private readonly XDocument _svgDocument;

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
    ///     Converts the SVG DisplayImage into a PNG. Up-scales it to the default size as part of the process.
    /// </summary>
    /// <returns>A PNG DisplayImage of the SVG</returns>
    public ImageSource AsImageSource(int rotation = 0, float scale = 1.0f) {
        var svg = new SKSvg();
        svg.Load(new MemoryStream(Encoding.UTF8.GetBytes(_svgDocument.ToString())));
        if (svg.Picture == null) throw new ApplicationException("Unable to load SVG or create Picture.");

        // Safe retrieval of dimensions
        var width = svg.Picture.CullRect.Width;
        var height = svg.Picture.CullRect.Height;
        if (width <= 0 || height <= 0) throw new ApplicationException("Invalid SVG picture dimensions.");

        // Maintain aspect-ratio when scaling
        const int quality = 100;
        var scaleX = DefaultWidth / width * scale;
        var scaleY = DefaultHeight / height * scale;

        var stream = new MemoryStream();
        svg.Save(stream, SKColor.Empty, SKEncodedImageFormat.Png, quality, scaleX, scaleY);
        stream.Seek(0, SeekOrigin.Begin);
        return ImageSource.FromStream(() => stream);
    }

    public SKCanvasView AsCanvasView(int rotation = 0, float scale = 1.5f) {
        var svg = new SKSvg();
        svg.Load(new MemoryStream(Encoding.UTF8.GetBytes(_svgDocument.ToString())));
        if (svg.Picture == null) throw new ApplicationException("Unable to load SVG or create Picture.");

        var canvasView = new SKCanvasView();
        canvasView.PaintSurface += (sender, e) => {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent); // Clear the canvas

            // Get the dimensions of the canvas surface and SVG
            var canvasWidth = e.Info.Width;
            var canvasHeight = e.Info.Height;
            var svgWidth = svg.Picture?.CullRect.Width;
            var svgHeight = svg.Picture?.CullRect.Height;

            // Calculate scale to fit the SVG into the canvas, maintaining aspect-ratio
            var scaleX = canvasWidth / svgWidth * scale ?? 1.0f;
            var scaleY = canvasHeight / svgHeight * scale ?? 1.0f;

            // Create transformation-matrix to center and scale the SVG
            var matrix = SKMatrix.CreateScale(scaleX, scaleY);
            var translateX = (canvasWidth - svgWidth * scaleX) / 2 ?? 0.0f;
            var translateY = (canvasHeight - svgHeight * scaleY) / 2 ?? 0.0f;
            matrix = SKMatrix.Concat(SKMatrix.CreateTranslation(translateX, translateY), matrix);

            // Apply rotation to the canvas
            canvas.Translate((float)canvasWidth / 2, (float)canvasHeight / 2);   // Move to the center of the canvas
            canvas.RotateDegrees(rotation);                                      // Rotate by the specified angle
            canvas.Translate((float)-canvasWidth / 2, (float)-canvasHeight / 2); // Move to the center of the canvas

            // Draw the SVG picture on the canvas
            canvas.DrawPicture(svg.Picture, in matrix);
        };
        return canvasView;
    }

    /// <summary>
    /// Draw with optional opacity, pixel offset, and blend mode.
    /// </summary>
    public void Draw(SKCanvas canvas, SKRect destWorld,
        float rotationDegrees = 0f,
        float opacity = 1f,
        SKPoint? offsetPx = null,
        SKBlendMode blend = SKBlendMode.SrcOver) {
        using var picture = ToPicture();
        if (picture == null) return;

        var svgRect = picture.CullRect;
        var sx = destWorld.Width / svgRect.Width;
        var sy = destWorld.Height / svgRect.Height;

        // Build transform for scale + placement
        var scale = SKMatrix.CreateScale(sx, sy);
        var translate = SKMatrix.CreateTranslation(
            destWorld.Left - svgRect.Left * sx + (offsetPx?.X ?? 0f),
            destWorld.Top - svgRect.Top * sy + (offsetPx?.Y ?? 0f)
        );
        var m = SKMatrix.Concat(translate, scale);

        canvas.Save();

        if (rotationDegrees != 0f) {
            canvas.Translate(destWorld.MidX, destWorld.MidY);
            canvas.RotateDegrees(rotationDegrees);
            canvas.Translate(-destWorld.MidX, -destWorld.MidY);
        }

        // Apply opacity/blend using a layer
        using var paint = new SKPaint { BlendMode = blend, Color = SKColors.White.WithAlpha((byte)(opacity * 255)) };
        canvas.SaveLayer(paint);
        canvas.DrawPicture(picture, in m);
        canvas.Restore(); // layer
        canvas.Restore(); // canvas
    }

    /// <summary>
    ///     Converts the XDocument SVG DisplayImage into a stream which can be consumed by
    ///     the ImageSource.FromStream method
    /// </summary>
    private static Stream ConvertXDocumentToStream(XDocument svgDocument) {
        var stream = new MemoryStream();
        svgDocument.Save(stream);
        stream.Position = 0;
        return stream;
    }

    private static XDocument LoadSvg(string resourceName) {
        try {
            using var stream = SvgImages.ExecutingAssembly.GetManifestResourceStream(resourceName);
            if (stream == null) {
                throw new FileNotFoundException("Resource not found.", resourceName);
            }
            return XDocument.Load(stream);
        } catch (Exception ex) {
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
    public List<XElement> FindElements(string elementName) => (from element in _svgDocument.Descendants() from attr in element.Attributes() where attr.Name.LocalName.Equals("id", StringComparison.OrdinalIgnoreCase) && attr.Value.Equals(elementName, StringComparison.OrdinalIgnoreCase) select element).ToList();

    public string ElementType(XElement element) => element.Name.LocalName.ToLowerInvariant();

    public bool IsElementOfType(XElement element, string type) => element.Name.LocalName.Equals(type, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Given an element, set the attribute property to the value provided
    /// </summary>
    public void SetAttributeValue(XElement element, string attributeName, string attributeValue, bool addIfNotExist = true) {
        ArgumentNullException.ThrowIfNull(element);
        var attribute = (from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr).FirstOrDefault();
        if (attribute is { }) {
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
        return(from attr in element.Attributes() where attr.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) select attr.Value).FirstOrDefault();
    }
}