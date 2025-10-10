using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Converters;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleLabelTile : Tile, ITileDrawable {
    public DrawableCircleLabelTile(CircleLabelEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not CircleLabelEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new CircleLabelDrawable(e)
        };

        gv.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class CircleLabelDrawable : IDrawable {
        private readonly CircleLabelEntity e;
        public CircleLabelDrawable(CircleLabelEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            // Outer stroke
            var outerStroke = (float)Math.Max(0, e.BorderWidth);
            if (outerStroke > 0f) {
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize = outerStroke;
                canvas.DrawEllipse(r.Inflate(new SizeF(-outerStroke / 2f)));
            }

            // Gap ring (white stroke inside)
            var gap = (float)Math.Max(0, e.BorderInnerGap);
            if (gap > 0f) {
                var gapRect = r.Inflate(new SizeF(-outerStroke - gap / 2f));
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = gap;
                canvas.DrawEllipse(gapRect);
            }

            // Inner circle (fill + inner border)
            var innerInset = outerStroke + gap;
            var innerRect = r.Inflate(new SizeF(-innerInset));
            if (e.BackgroundColor is { } fill) {
                canvas.FillColor = fill;
                canvas.FillEllipse(innerRect);
            }

            var innerStroke = (float)Math.Max(0, e.BorderInnerWidth);
            if (innerStroke > 0f) {
                canvas.StrokeColor = e.BorderInnerColor;
                canvas.StrokeSize = innerStroke;
                canvas.DrawEllipse(innerRect.Inflate(new SizeF(-innerStroke / 2f)));
            }

            // Centered text (SkiaSharp 3.x — fit inside innerRect, centered)
            if (!string.IsNullOrWhiteSpace(e.Label)) {
                // Resolve face
                var typeface = ResolveTypeface(e.FontAlias);

                // Requested size via your unit scale (same as TextTile)
                float requested = FontSizeHelper.UnitToSize(e.FontSize <= 0 ? 14f : e.FontSize);

                using var font = new SkiaSharp.SKFont(typeface, requested) { Subpixel = true, LinearMetrics = true };
                using var paint = new SkiaSharp.SKPaint { IsAntialias = true, Color = ToSkColor(e.TextColor) };

                // Measure tight glyph bounds at requested size (do not change font.Size afterward)
                font.MeasureText(e.Label, out SkiaSharp.SKRect tight);
                float W = tight.Width;
                float H = tight.Height;

                // Rotated-box math (no rotation for circle label presumed; if you add rotation later,
                // follow the TextTile's rotate+scale placement pattern)
                // We treat the unrotated tight box corners and center that in innerRect.
                float minX = 0f;
                float minY = 0f;
                float maxX = W;
                float maxY = H;

                float spanX = maxX - minX;
                float spanY = maxY - minY;

                // Downscale only if needed to fit innerRect
                float s = MathF.Min(1f,
                    MathF.Min(
                        innerRect.Width / MathF.Max(1e-6f, spanX),
                        innerRect.Height / MathF.Max(1e-6f, spanY)));

                // Place centered in innerRect
                float Tx = innerRect.Center.X - s * (minX + maxX) * 0.5f;
                float Ty = innerRect.Center.Y - s * (minY + maxY) * 0.5f;

                // Local draw: top-left origin at (0,0)
                float xLocal = -tight.Left;
                float baselineLocalY = -tight.Top;

                // We need a native Skia canvas to draw; GraphicsView's ICanvas wraps one internally,
                // but doesn't expose it. Easiest path: draw text via Skia using a snapshot layer.
                // Create a temporary SKCanvas over the MAUI ICanvas via a Picture/Recording:
                using var recorder = new SkiaSharp.SKPictureRecorder();
                var picRect = new SkiaSharp.SKRect(0, 0, innerRect.Width, innerRect.Height);
                var skCanvas = recorder.BeginRecording(picRect);

                // Translate/scale to our placement inside the inner rect
                skCanvas.Translate(Tx - innerRect.Left, Ty - innerRect.Top);
                if (Math.Abs(s - 1f) > 1e-6f) skCanvas.Scale(s, s);

                // Draw the text centered (no alignment options required)
                skCanvas.DrawText(e.Label, xLocal, baselineLocalY, SkiaSharp.SKTextAlign.Left, font, paint);

                // Finish recording and draw the picture back onto the MAUI canvas
                using var picture = recorder.EndRecording();

                // Convert to an image and paint into the MAUI canvas rect area
                using var skImage = SkiaSharp.SKImage.FromPicture(picture, new SkiaSharp.SKSizeI((int)innerRect.Width, (int)innerRect.Height));
                using var skData = skImage.Encode();
                var image = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(skData.AsStream());
                canvas.DrawImage(image,
                    innerRect.X,
                    innerRect.Y,
                    innerRect.Width,
                    innerRect.Height);
            }

            canvas.RestoreState();
        }
    }

    private static SKTypeface ResolveTypeface(string? alias) {
        // Try filename from your catalog first (most reliable)
        if (!string.IsNullOrWhiteSpace(alias)) {
            var meta = FontCatalog.GetFontFace(alias);
            if (meta != null) {
                try {
                    using var s = FileSystem.OpenAppPackageFileAsync(meta.Filename).GetAwaiter().GetResult();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    ms.Position = 0;
                    var tf = SKTypeface.FromStream(ms);
                    if (tf != null) return tf;
                } catch { /* fall through */
                }
            }
        }

        // Fall back to family/alias name, then default
        return SKFontManager.Default.MatchFamily(alias ?? string.Empty)
            ?? SKTypeface.FromFamilyName(alias ?? string.Empty)
            ?? SKTypeface.Default;
    }

    private static SkiaSharp.SKColor ToSkColor(Microsoft.Maui.Graphics.Color? c) {
        if (c is null) return SkiaSharp.SKColors.Transparent;
        return new SkiaSharp.SKColor(
            (byte)(c.Red * 255),
            (byte)(c.Green * 255),
            (byte)(c.Blue * 255),
            (byte)(c.Alpha * 255));
    }
}