using System.IO;
using System.Xml.Linq;
using SkiaSharp;
using Svg.Skia;

namespace DCCPanelController.Models.ViewModel.ImageManager {
    // Add 'partial' to your original class: public partial class SvgImageManager { ... }
    public partial class SvgImageManager {
        /// <summary>
        /// Returns the current styled SVG as a MemoryStream (caller owns the stream).
        /// </summary>
        public MemoryStream ToSvgStream(bool pretty = false) {
            var ms = new MemoryStream();
            _svgDocument.Save(ms, pretty ? SaveOptions.None : SaveOptions.DisableFormatting);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Returns the current styled SVG as an XML string.
        /// </summary>
        public string ToSvgString(bool pretty = false) {
            using var ms = ToSvgStream(pretty);
            using var reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Builds an SKPicture from the current styled SVG (caller should Dispose()).
        /// </summary>
        public SKPicture? ToPicture() {
            using var ms = ToSvgStream();
            var svg = new SKSvg();
            svg.Load(ms);
            return svg.Picture;
        }

        /// <summary>
        /// Draws the current styled SVG into the given world rect, with optional rotation (degrees).
        /// </summary>
        public void Draw(SKCanvas canvas, SKRect destWorld, float rotationDegrees = 0f) {
            using var picture = ToPicture();
            if (picture == null) return;

            var svgRect = picture.CullRect;
            var sx = destWorld.Width / svgRect.Width;
            var sy = destWorld.Height / svgRect.Height;

            canvas.Save();
            if (rotationDegrees != 0f) {
                canvas.Translate(destWorld.MidX, destWorld.MidY);
                canvas.RotateDegrees(rotationDegrees);
                canvas.Translate(-destWorld.MidX, -destWorld.MidY);
            }

            var scale = SKMatrix.CreateScale(sx, sy);
            var translate = SKMatrix.CreateTranslation(destWorld.Left - svgRect.Left * sx, destWorld.Top - svgRect.Top * sy);
            var m = SKMatrix.Concat(translate, scale);
            canvas.DrawPicture(picture, in m);
            canvas.Restore();
        }
    }
}