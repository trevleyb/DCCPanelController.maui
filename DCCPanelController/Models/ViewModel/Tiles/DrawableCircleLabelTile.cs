using System;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Converters;
using DCCPanelController.View.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleLabelTile : Tile, ITileDrawable {
    public DrawableCircleLabelTile(CircleLabelEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not CircleLabelEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var view = new SKCanvasView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            IgnorePixelScaling = true,   // <— IMPORTANT: draw in DIPs, not raw pixels
            EnableTouchEvents = false
        };
        
        view.PaintSurface += (_, args) => {
            var c = args.Surface.Canvas;
            var r = new SKRect(0, 0, args.Info.Width, args.Info.Height);
            c.Clear(ToSkColor(e.BackgroundColor));

            // --- draw circles (kept simple; adapt to your original if needed) ---
            using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(2, Math.Min(r.Width, r.Height) * 0.03f), Color = SKColors.Gray };
            var radius = Math.Min(r.Width, r.Height) * 0.4f;
            c.DrawCircle(r.MidX, r.MidY, radius, circlePaint);

            // --- center text in the middle ---
            if (!string.IsNullOrWhiteSpace(e.Label)) {
                
                // --- resolve face & build font/paint (keep your code) ---
                var typeface = ResolveTypeface(e.FontAlias ?? FontCatalog.DefaultFontAlias);
                var requestedPx = FontSizeHelper.UnitToSize(e.FontSize <= 0 ? 14f : (float)e.FontSize);
                using var font = new SKFont(typeface, requestedPx) { Subpixel = true };
                using var p = new SKPaint { IsAntialias = true, Color = ToSkColor(e.TextColor) };

                // --- TIGHT BOUNDS (correct) ---
                font.MeasureText(e.Label, out SKRect tight); // tight.Left/Top can be negative
                float textW = tight.Width;                   // <- use tight bounds, not advance
                float textH = tight.Height;

                // --- fit into inner box with rotation taken into account ---
                var inner = new SKRect(
                    r.Left + r.Width * 0.10f,
                    r.Top + r.Height * 0.10f,
                    r.Right - r.Width * 0.10f,
                    r.Bottom - r.Height * 0.10f);

                float rad = (float)((e.Rotation % 360) * Math.PI / 180.0);
                float cos = Math.Abs((float)Math.Cos(rad));
                float sin = Math.Abs((float)Math.Sin(rad));
                float rotatedW = textW * cos + textH * sin;
                float rotatedH = textW * sin + textH * cos;

                float availW = Math.Max(1f, inner.Width);
                float availH = Math.Max(1f, inner.Height);
                float scale = Math.Min(availW / rotatedW, availH / rotatedH);

                if (scale < 1f) {
                    font.Size *= scale;

                    // recompute tight bounds for new size
                    font.MeasureText(e.Label, out tight);
                    textW = tight.Width;
                    textH = tight.Height;
                }

                // --- rotation around center ---
                c.Save();
                c.Translate(r.MidX, r.MidY);
                c.RotateDegrees((float)(e.Rotation % 360));
                c.Translate(-r.MidX, -r.MidY);

                // --- horizontal alignment (pass to DrawText, not on SKPaint) ---
                var align = SKTextAlign.Center; // your tile is centered; map if you expose H align

                // --- baseline from bounds (this is the key) ---
                // DrawText positions by baseline Y. tight.Top is usually negative.
                // To center vertically: choose y so that (y + tight.Top + tight.Bottom)/2 == r.MidY
                float baselineCenter = r.MidY - (tight.Top + tight.Bottom) * 0.5f;

                // If you support Top/Bottom vertical align, use:
                //   Top:    baseline = inner.Top    - tight.Top
                //   Bottom: baseline = inner.Bottom - tight.Bottom
                //   Center: baseline = r.MidY       - (tight.Top + tight.Bottom)/2

                // --- draw (modern overload; no SKPaint.TextAlign) ---
                c.DrawText(e.Label, r.MidX, baselineCenter, align, font, p);

                c.Restore();
            }
        };

        view.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
        view.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        view.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => view.InvalidateSurface();
        return view;
    }

    private static float BaselineForCenter(SKRect r, SKFontMetrics m) {
        float half = (m.Descent - m.Ascent) * 0.5f;
        return r.MidY + half + m.Ascent;
    }

    private static SKTypeface ResolveTypeface(string? alias) {
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

        return SKFontManager.Default.MatchFamily(alias ?? string.Empty)
            ?? SKTypeface.FromFamilyName(alias ?? string.Empty)
            ?? SKTypeface.Default;
    }

    private static SKColor ToSkColor(Microsoft.Maui.Graphics.Color? c) {
        if (c is null) return SKColors.Transparent;
        return new SKColor(
            (byte)(c.Red * 255),
            (byte)(c.Green * 255),
            (byte)(c.Blue * 255),
            (byte)(c.Alpha * 255));
    }

    private static float DegreesToRadians(float deg) => (float)(Math.PI / 180.0) * deg;
}