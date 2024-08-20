using System.Reflection;
using System.Text;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace DCCPanelController.Components.Tracks.SVGManager;

public class SvgImageManager {

    private const int DefaultWidth = 192;
    private const int DefaultHeight = 192;
    private const string StyleAttribute = "style=";
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

    private ISvgModifier StyleModifier => _svgImageXDoc.ToString().Contains(StyleAttribute) ? new SvgStyle(_svgImageXDoc) : new SvgAttribute(_svgImageXDoc);

    public void SetElementOccupied(Color? color, int? opacity = null) => StyleModifier.SetElementOccupied(color, opacity);
    public void SetElementFree(Color? color, int? opacity = null) => StyleModifier.SetElementFree(color, opacity);
    public void SetElementRoute(Color? color, int? opacity = null) => StyleModifier.SetElementRoute(color, opacity);
    public void SetButtonColor(Color? color, int? opacity = null) => StyleModifier.SetButtonColor(color, opacity);
    public void SetTrackColor(Color? color, int? opacity = null) => StyleModifier.SetTrackColor(color, opacity);
    public void SetBorderColor(Color? color, int? opacity = null) => StyleModifier.SetBorderColor(color, opacity);
    public void SetDivergingColor(Color? color, int? opacity = null) => StyleModifier.SetDivergingColor(color, opacity);
    public void SetTerminatorColor(Color? color, int? opacity = null) => StyleModifier.SetTerminatorColor(color, opacity);
    public void SetContinuationColor(Color? color, int? opacity = null) => StyleModifier.SetContinuationColor(color, opacity);
}
