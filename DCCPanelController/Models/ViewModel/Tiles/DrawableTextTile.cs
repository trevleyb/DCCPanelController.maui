using System.Text;
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
            .Track(nameof(TextEntity.FontAlias), () => entity.FontAlias)
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

        view.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
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

        // --- Explicit, adjustable padding (same semantics you asked for) ---
        float hPad = 2f; // horizontal pad in px
        float vPad = 0f; // vertical pad in px (top/bottom)
        var inner = new SKRect(r.Left + hPad, r.Top + vPad, r.Right - hPad, r.Bottom - vPad);

        // Short-circuit: nothing to draw
        if (inner.Width <= 0 || inner.Height <= 0) return;

        string text = e.Label;

        // Measure single line tight bounds
        font.MeasureText(text, out SKRect tightOne);
        float oneW = tightOne.Width;

        // Prepare metrics
        var fm = font.Metrics;                        // ascent is negative, descent positive
        float baseLineGap = fm.Descent - fm.Ascent;   // height for first line’s box
        float lineAdvance = baseLineGap + fm.Leading; // preferred distance between baselines

        // Wrap if requested and beneficial (basic heuristic: single line overshoots inner width)
        List<string> lines;
        if (e.WrapText && text.IndexOf(' ') >= 0 && oneW > inner.Width) {
            lines = WrapLinesByWidth(text, font, inner.Width);
            if (lines.Count == 0) lines = new List<string> { text };
        } else {
            lines = new List<string> { text };
        }

        // Compute multi-line block width & height in local (unrotated) space
        float blockW = 0f;
        foreach (var ln in lines) {
            font.MeasureText(ln, out SKRect t);
            blockW = MathF.Max(blockW, t.Width);
        }

        // Total height from first baseline to last glyph bottom:
        //   first baseline sits at -fm.Ascent
        //   last line bottom is at ((lines-1)*lineAdvance + fm.Descent)
        float blockH = ((lines.Count - 1) * lineAdvance) + (fm.Descent - fm.Ascent);

        // We’ll draw using a local origin at the *top-left* of the block’s glyph box.
        // That means:
        //   - First line baseline = -fm.Ascent
        //   - For line i: baselineY = -fm.Ascent + i*lineAdvance
        // Horizontal per-line alignment will adjust x.
        float localOriginX = 0f;
        float localOriginY = 0f;

        // Rotation math for the (unrotated) block rectangle A(0,0) → B(blockW, blockH)
        float theta = DegreesToRadians((float)(e.Rotation % 360f));
        float cosT = (float)Math.Cos(theta);
        float sinT = (float)Math.Sin(theta);

        static void Rot(float x, float y, float c, float s, out float rx, out float ry) {
            rx = x * c - y * s;
            ry = x * s + y * c;
        }

        Rot(0, 0, cosT, sinT, out var x1, out var y1);
        Rot(blockW, 0, cosT, sinT, out var x2, out var y2);
        Rot(blockW, blockH, cosT, sinT, out var x3, out var y3);
        Rot(0, blockH, cosT, sinT, out var x4, out var y4);

        float minX = MathF.Min(MathF.Min(x1, x2), MathF.Min(x3, x4));
        float maxX = MathF.Max(MathF.Max(x1, x2), MathF.Max(x3, x4));
        float minY = MathF.Min(MathF.Min(y1, y2), MathF.Min(y3, y4));
        float maxY = MathF.Max(MathF.Max(y1, y2), MathF.Max(y3, y4));

        float spanX = maxX - minX;
        float spanY = maxY - minY;

        // Uniform DOWN-scale so the rotated multi-line block fits within the inner rect
        float s = MathF.Min(1f,
            MathF.Min(
                inner.Width / MathF.Max(1e-6f, spanX),
                inner.Height / MathF.Max(1e-6f, spanY)));

        // Compute world translation so AFTER rotate+scale, the rotated block aligns to inner based on H/V alignment
        float Tx; // world translation X
        float Ty; // world translation Y

        // Horizontal block alignment: left/center/right against the rotated bounding box
        switch (e.HorizontalJustification) {
        case TextAlignmentHorizontalEnum.Left:
            Tx = inner.Left - s * minX;
            break;
        case TextAlignmentHorizontalEnum.Right:
            Tx = inner.Right - s * maxX;
            break;
        case TextAlignmentHorizontalEnum.Justified: // treat block as centered; line-level justification handled below
        case TextAlignmentHorizontalEnum.Center:
        default:
            Tx = inner.MidX - s * (minX + maxX) * 0.5f;
            break;
        }

        // Vertical block alignment: top/center/bottom against the rotated bounding box
        switch (e.VerticalJustification)
        {
        case TextAlignmentVerticalEnum.Top:
            // place top of text block exactly at inner.Top
            Ty = inner.Top - s * minY;
            break;

        case TextAlignmentVerticalEnum.Bottom:
            // place bottom of text block exactly at inner.Bottom
            Ty = inner.Bottom - s * maxY;
            break;

        case TextAlignmentVerticalEnum.Center:
        default:
            // true vertical centering
            Ty = inner.MidY - s * (minY + maxY) * 0.5f;
            break;
        }
        // Draw
        c.Save();
        c.Translate(Tx, Ty);
        c.RotateDegrees((float)(e.Rotation % 360f));
        if (Math.Abs(s - 1f) > 1e-6f) c.Scale(s, s);

        // Per-line draw with horizontal alignment (and optional simple justification for single-line only)
        bool blockJustified = e.HorizontalJustification == TextAlignmentHorizontalEnum.Justified;

        for (int i = 0; i < lines.Count; i++) {
            var line = lines[i];
            font.MeasureText(line, out SKRect lineTight);
            float lineW = lineTight.Width;

            float x;
            if (blockJustified && lines.Count == 1) {
                // single-line justification (original behavior)
                DrawJustifiedSingleLine(c, line, localOriginX, -fm.Ascent, lineW, blockW, font, paint);
                continue;
            }

            // Left/Center/Right within the block width
            switch (e.HorizontalJustification) {
            case TextAlignmentHorizontalEnum.Left:
                x = localOriginX;
                break;
            case TextAlignmentHorizontalEnum.Right:
                x = localOriginX + (blockW - lineW);
                break;
            case TextAlignmentHorizontalEnum.Center:
            default:
                x = localOriginX + (blockW - lineW) * 0.5f;
                break;
            }

            float baselineY = localOriginY - fm.Ascent + i * lineAdvance;
            c.DrawText(line, x - lineTight.Left, baselineY, SKTextAlign.Left, font, paint);
        }

        c.Restore();
    }

    // Greedy word wrap: fills lines not exceeding maxWidth (at the requested font size). If a single word
    // exceeds maxWidth, it will be placed alone on a line and the block will be uniformly scaled later.
    private static List<string> WrapLinesByWidth(string text, SKFont font, float maxWidth) {
        var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        if (words.Length == 0) return lines;

        var current = new StringBuilder(words[0]);
        for (int i = 1; i < words.Length; i++) {
            string candidate = current.Length == 0 ? words[i] : current + " " + words[i];

            font.MeasureText(candidate, out SKRect tight);
            if (tight.Width <= maxWidth) {
                current.Clear();
                current.Append(candidate);
            } else {
                // commit current line
                lines.Add(current.ToString());
                current.Clear();
                current.Append(words[i]);
            }
        }

        if (current.Length > 0) lines.Add(current.ToString());
        return lines;
    }

    // Original single-line justification logic extracted for reuse
    private static void DrawJustifiedSingleLine(SKCanvas c, string text, float xLocal, float baselineLocalY,
        float measuredWidth, float targetWidth, SKFont font, SKPaint paint) {
        var words = text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        int gaps = Math.Max(0, words.Length - 1);
        if (gaps == 0 || targetWidth <= measuredWidth + 1e-3f) {
            c.DrawText(text, xLocal, baselineLocalY, SKTextAlign.Left, font, paint);
            return;
        }

        float spaceAdv = font.MeasureText(" ", out _);
        float wordsAdv = 0f;
        foreach (var w in words) wordsAdv += font.MeasureText(w, out _);

        float extra = MathF.Max(0f, targetWidth - (wordsAdv + gaps * spaceAdv));
        float perGap = extra / gaps;

        float cursor = xLocal;
        for (int i = 0; i < words.Length; i++) {
            var w = words[i];
            c.DrawText(w, cursor, baselineLocalY, SKTextAlign.Left, font, paint);
            cursor += font.MeasureText(w, out _);
            if (i < words.Length - 1) cursor += spaceAdv + perGap;
        }
    }

    private static void DrawText_remove(SKCanvas c, SKRect r, TextEntity e, float gridSize /*unused*/) {
        if (string.IsNullOrWhiteSpace(e.Label)) return;

        // Typeface + requested size (DIPs)
        var tf = ResolveTypeface(e.FontAlias);
        float requested = FontSizeHelper.UnitToSize(e.FontSize <= 0 ? 14f : e.FontSize);

        using var font = new SKFont(tf, requested) { Subpixel = true, LinearMetrics = true };
        using var paint = new SKPaint { IsAntialias = true, Color = ToSkColor(e.TextColor) };

        // --- Explicit, adjustable padding ---
        float hPad = 2f;
        float vPad = 0f;

        // Work inside a padded rect
        var inner = new SKRect(r.Left + hPad, r.Top + vPad, r.Right - hPad, r.Bottom - vPad);

        var text = e.Label;

        // Tight glyph bounds at requested size (DO NOT change font size after this)
        font.MeasureText(text, out SKRect tight);
        float W = tight.Width;
        float H = tight.Height;

        // DrawText uses a baseline; move (0,0) to tight top-left so xLocal/yLocal are intuitive
        float xLocal = -tight.Left;
        float baselineLocalY = -tight.Top;

        // Rotation math for the tight box corners
        float theta = DegreesToRadians((float)(e.Rotation % 360f));
        float cosT = (float)Math.Cos(theta);
        float sinT = (float)Math.Sin(theta);

        static void Rot(float x, float y, float c, float s, out float rx, out float ry) {
            rx = x * c - y * s;
            ry = x * s + y * c;
        }

        Rot(0, 0, cosT, sinT, out var x1, out var y1);
        Rot(W, 0, cosT, sinT, out var x2, out var y2);
        Rot(W, H, cosT, sinT, out var x3, out var y3);
        Rot(0, H, cosT, sinT, out var x4, out var y4);

        float minX = MathF.Min(MathF.Min(x1, x2), MathF.Min(x3, x4));
        float maxX = MathF.Max(MathF.Max(x1, x2), MathF.Max(x3, x4));
        float minY = MathF.Min(MathF.Min(y1, y2), MathF.Min(y3, y4));
        float maxY = MathF.Max(MathF.Max(y1, y2), MathF.Max(y3, y4));

        float spanX = maxX - minX;
        float spanY = maxY - minY;

        // Uniform DOWN-scale so the rotated box fits within the padded tile area
        float s = MathF.Min(1f,
            MathF.Min(
                inner.Width / MathF.Max(1e-6f, spanX),
                inner.Height / MathF.Max(1e-6f, spanY)));

        // Compute Tx, Ty so AFTER rotate+scale the rotated box sits where we want
        float Tx; // world translation X
        float Ty; // world translation Y

        // Horizontal alignment (unchanged)
        switch (e.HorizontalJustification) {
        case TextAlignmentHorizontalEnum.Left:
            Tx = inner.Left - s * minX;
            break;
        case TextAlignmentHorizontalEnum.Right:
            Tx = inner.Right - s * maxX;
            break;
        case TextAlignmentHorizontalEnum.Justified:
        case TextAlignmentHorizontalEnum.Center:
        default:
            Tx = inner.MidX - s * (minX + maxX) * 0.5f;
            break;
        }

        // Vertical alignment — now exact to the TILE top/bottom with a 1px pad
        switch (e.VerticalJustification) {
        case TextAlignmentVerticalEnum.Top:
            // top of rotated box touches r.Top + vPad
            Ty = (r.Top + vPad) - s * minY;
            break;
        case TextAlignmentVerticalEnum.Bottom:
            // bottom of rotated box touches r.Bottom - vPad
            Ty = (r.Bottom - vPad) - s * maxY;
            break;
        case TextAlignmentVerticalEnum.Center:
        default:
            // unchanged: center the rotated box within the padded rect
            Ty = inner.MidY - s * (minY + maxY) * 0.5f;
            break;
        }

        // Draw
        c.Save();
        c.Translate(Tx, Ty);
        c.RotateDegrees((float)(e.Rotation % 360f));
        if (Math.Abs(s - 1f) > 1e-6f)
            c.Scale(s, s);

        bool justified = e.HorizontalJustification == TextAlignmentHorizontalEnum.Justified;

        if (!justified) {
            c.DrawText(text, xLocal, baselineLocalY, SKTextAlign.Left, font, paint);
        } else {
            // Simple word-spacing justification across the tight width W
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