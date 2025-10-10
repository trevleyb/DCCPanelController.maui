using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableTextTile : Tile, ITileDrawable {
    public DrawableTextTile(TextEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
            .Track(nameof(TextEntity.HorizontalJustification), () => entity.HorizontalJustification)
            .Track(nameof(TextEntity.VerticalJustification), () => entity.VerticalJustification)
            .Track(nameof(TextEntity.BackgroundColor), () => entity.BackgroundColor)
            .Track(nameof(TextEntity.FontSize), () => entity.FontSize)
            .Track(nameof(TextEntity.FontStyle), () => entity.FontStyle)
            .Track(nameof(TextEntity.Label), () => entity.Label)
            .Track(nameof(TextEntity.TextColor), () => entity.TextColor);
    }

    protected override Microsoft.Maui.Controls.View CreateTile() {
        if (Entity is not TextEntity e) throw new TileRenderException(GetType(), Entity.GetType());
        if (string.IsNullOrEmpty(e.Label)) e.Label = "T";

        var view = new SKCanvasView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            IgnorePixelScaling = true,
            EnableTouchEvents = false,
        };

        view.PaintSurface += (_, args) => {
            var c = args.Surface.Canvas;
            var cs = view.CanvasSize;
            var r = new SKRect(0, 0, (float)cs.Width, (float)cs.Height);
            DrawBackground(c, r, e);
            DrawText(c, r, e, (float)GridSize);
        };

        view.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        view.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, _) => view.InvalidateSurface();
        return view;
    }

    private static void DrawBackground(SKCanvas c, SKRect r, TextEntity e) {
        c.Clear(ToSkColor(e.BackgroundColor)); // fills even when null → transparent
    }

    private static void DrawText(SKCanvas c, SKRect r, TextEntity e, float gridSize /*unused*/) {
        if (string.IsNullOrWhiteSpace(e.Label)) return;

        // Typeface + requested size (DIPs)
        var tf = ResolveTypeface(e.FontAlias);
        float requested = FontSizeHelper.UnitToSize(e.FontSize <= 0 ? 14f : e.FontSize);

        using var font = new SKFont(tf, requested) { Subpixel = true, LinearMetrics = true };
        using var paint = new SKPaint { IsAntialias = true, Color = ToSkColor(e.TextColor) };

        // Work inside an inner rect
        const float pad = 2f;
        var inner = new SKRect(r.Left + pad, r.Top + pad, r.Right - pad, r.Bottom - pad);

        var text = e.Label;

        // Tight glyph bounds at requested size (DO NOT change font size after this)
        font.MeasureText(text, out SKRect tight);
        float W = tight.Width;
        float H = tight.Height;

        // We'll draw with the text's *top-left* as local origin (0,0)
        // Hence local baseline is (-tight.Top), and local "left" is (-tight.Left).
        float xLocal = -tight.Left;
        float baselineLocalY = -tight.Top;

        // Rotation
        float theta = DegreesToRadians((float)(e.Rotation % 360f));
        float cs = (float)Math.Cos(theta);
        float sn = (float)Math.Sin(theta);

        // Rotated corners of the tight box relative to (0,0) top-left
        // TL(0,0), TR(W,0), BR(W,H), BL(0,H)
        static void Rot(float x, float y, float c, float s, out float rx, out float ry) {
            rx = x * c - y * s;
            ry = x * s + y * c;
        }

        Rot(0, 0, cs, sn, out var x1, out var y1);
        Rot(W, 0, cs, sn, out var x2, out var y2);
        Rot(W, H, cs, sn, out var x3, out var y3);
        Rot(0, H, cs, sn, out var x4, out var y4);

        float minX = MathF.Min(MathF.Min(x1, x2), MathF.Min(x3, x4));
        float maxX = MathF.Max(MathF.Max(x1, x2), MathF.Max(x3, x4));
        float minY = MathF.Min(MathF.Min(y1, y2), MathF.Min(y3, y4));
        float maxY = MathF.Max(MathF.Max(y1, y2), MathF.Max(y3, y4));

        float spanX = maxX - minX;
        float spanY = maxY - minY;

        // Uniform downscale so rotated box fits inside inner
        float s = MathF.Min(1f,
            MathF.Min(
                inner.Width / MathF.Max(1e-6f, spanX),
                inner.Height / MathF.Max(1e-6f, spanY)));

        // Place the rotated box inside 'inner' according to alignment:
        // We compute (Tx, Ty) so that after rotate+scale, the rotated box's
        // min/max aligns to inner.Left/Center/Right (and Top/Center/Bottom).
        float Tx; // translation in world space before rotate/scale
        float Ty;

        // Horizontal
        switch (e.HorizontalJustification) {
        case TextAlignmentHorizontalEnum.Left:
            // left edge of rotated box sits on inner.Left
            Tx = inner.Left - s * minX;
            break;
        case TextAlignmentHorizontalEnum.Right:
            // right edge of rotated box sits on inner.Right
            Tx = inner.Right - s * maxX;
            break;
        case TextAlignmentHorizontalEnum.Justified:
        case TextAlignmentHorizontalEnum.Center:
        default:
            // horizontal center of rotated box at inner.MidX
            Tx = inner.MidX - s * (minX + maxX) * 0.5f;
            break;
        }

        // Vertical
        switch (e.VerticalJustification) {
        case TextAlignmentVerticalEnum.Top:
            Ty = inner.Top - s * minY;
            break;
        case TextAlignmentVerticalEnum.Bottom:
            Ty = inner.Bottom - s * maxY;
            break;
        case TextAlignmentVerticalEnum.Center:
        default:
            Ty = inner.MidY - s * (minY + maxY) * 0.5f;
            break;
        }

        // Draw: translate to placement, then rotate and scale about the (0,0) local origin
        c.Save();
        c.Translate(Tx, Ty);
        c.RotateDegrees((float)(e.Rotation % 360f));
        if (Math.Abs(s - 1f) > 1e-6f)
            c.Scale(s, s);

        bool justify = e.HorizontalJustification == TextAlignmentHorizontalEnum.Justified;

        if (!justify) {
            c.DrawText(text, xLocal, baselineLocalY, SKTextAlign.Left, font, paint);
        } else {
            // Word-spacing justification across the unscaled glyph width W
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int gaps = Math.Max(0, words.Length - 1);

            if (gaps == 0) {
                c.DrawText(text, xLocal, baselineLocalY, SKTextAlign.Left, font, paint);
            } else {
                float spaceAdv = font.MeasureText(" ", out _);
                float wordsAdv = 0f;
                foreach (var w in words) wordsAdv += font.MeasureText(w, out _);

                float extra = MathF.Max(0f, W - (wordsAdv + gaps * spaceAdv));
                float perGap = extra / gaps;

                float cursor = xLocal;
                for (int i = 0; i < words.Length; i++) {
                    var w = words[i];
                    c.DrawText(w, cursor, baselineLocalY, SKTextAlign.Left, font, paint);
                    cursor += font.MeasureText(w, out _);
                    if (i < words.Length - 1) cursor += spaceAdv + perGap;
                }
            }
        }

        c.Restore();
    }

    private static float BaselineForTop(SKRect r, SKFontMetrics m, float pad) => r.Top + pad - m.Ascent;

    private static float BaselineForCenter(SKRect r, SKFontMetrics m) {
        float boxHalf = (m.Descent - m.Ascent) * 0.5f;

        // Center the glyph box around MidY: baseline = center + half + Ascent
        return r.MidY + boxHalf + m.Ascent;
    }

    private static float BaselineForBottom(SKRect r, SKFontMetrics m, float pad) => r.Bottom - pad - m.Descent;

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