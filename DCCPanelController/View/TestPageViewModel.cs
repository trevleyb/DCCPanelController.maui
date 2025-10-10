using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Graphics.Text;
using Font = Microsoft.Maui.Graphics.Font;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using SKCanvasView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Linq;


namespace DCCPanelController.View;

public struct SomeTestItem {
    public required string Name { get; set; }
    public required string Id { get; set; }
}

public partial class TestPageViewModel : ObservableObject {
    [ObservableProperty] private string? _selectedFontAlias = "OpenSans-Regular";
    [ObservableProperty] private string? _selectedFontStyle;
    [ObservableProperty] private string? _selectedFontFamily;
    [ObservableProperty] private int     _fontSize = 12;

    // in TestPageViewModel.cs (same namespace)
    public sealed class TextSmokeTestDrawable : IDrawable {
        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            // white background + a frame so we know we’re drawing
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(r);
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(r);

            // baseline guides to ensure we see *something*
            float midY = r.Center.Y;
            canvas.StrokeColor = Colors.Orange;
            canvas.DrawLine(r.Left + 10, midY, r.Right - 10, midY);

            // DEFAULT FONT ONLY – if this doesn’t render, the text backend isn’t drawing
            canvas.Font = Font.Default;
            canvas.FontColor = Colors.Blue;
            canvas.FontSize = 28;

            // Use the point overload (no clip rect), center on our guide line
            //canvas.DrawString("HELLO (MAUI Graphics)", r.Center.X, midY, HorizontalAlignment.Center);
            var attrText = new AttributedText("HELLO (MAUI Graphics)", null);
            canvas.DrawText(attrText, r.Center.X, midY, r.Width, r.Height);
            canvas.RestoreState();
        }
    }

    // quick view to host it:
    public Microsoft.Maui.Controls.View TextSmokeTest =>
        new GraphicsView {
            HeightRequest = 200,
            WidthRequest = 600,
            Drawable = new TextSmokeTestDrawable(),
            BackgroundColor = Colors.White
        };

    public Microsoft.Maui.Controls.View TestFontsMaui =>
        new GraphicsView {
            HeightRequest = 500,
            WidthRequest = 600,
            Drawable = new PagedFontProbeDrawableAttributed(FontCatalog.RegisteredFonts.Select(f => f.Alias).ToArray()),
            BackgroundColor = Colors.White
        };

    public Microsoft.Maui.Controls.View TestFontsSkia =>
        new PagedFontProbeSkiaView(FontCatalog.RegisteredFonts.Select(f => f.Alias).ToArray()) {
            HeightRequest = 500,
            WidthRequest = 600,
            BackgroundColor = Colors.White
        };

    // using Microsoft.Maui.Graphics;
// using Microsoft.Maui.Graphics.Text;
// using System.Linq;

    public sealed class PagedFontProbeDrawableAttributed : IDrawable {
        private readonly string[] _aliases;
        private readonly int      _start;
        private readonly int      _count;

        public PagedFontProbeDrawableAttributed(string[] aliases, int startIndex = 0, int count = 10) {
            _aliases = aliases ?? Array.Empty<string>();
            _start = Math.Max(0, startIndex);
            _count = Math.Max(1, count);
        }

        public void Draw(ICanvas canvas, RectF rect) {
            canvas.SaveState();
            canvas.Antialias = true;

            // background
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(rect);

            if (_aliases.Length == 0) {
                DrawOne(canvas, rect, "OpenSans", 18f, "No fonts registered", Colors.Red);
                canvas.RestoreState();
                return;
            }

            // Title
            var pad = 10f;
            var y = rect.Top + pad;
            var end = Math.Min(_start + _count, _aliases.Length);
            var titleRect = new RectF(rect.Left + pad, y, rect.Width - pad * 2, 26f);
            DrawOne(canvas, titleRect, _aliases[0], 14f, $"Font Probe ({_start + 1}–{end} of {_aliases.Length})", Colors.Gray);
            y += titleRect.Height + pad;

            // Rows
            int visible = end - _start;
            if (visible <= 0) {
                canvas.RestoreState();
                return;
            }

            float availH = rect.Bottom - y - pad;
            float rowH = Math.Max(36f, availH / visible);

            for (int i = _start; i < end; i++) {
                var row = new RectF(rect.Left + pad, y + (i - _start) * rowH, rect.Width - pad * 2, rowH - 6f);

                // border
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(row);

                var alias = _aliases[i];
                var bigSize = Math.Clamp(row.Height * 0.52f, 12f, 46f);
                var smlSize = Math.Clamp(row.Height * 0.34f, 10f, 30f);

                // top (big)
                var topRect = new RectF(row.X, row.Y, row.Width, row.Height * 0.58f);
                DrawOne(canvas, topRect, alias, bigSize, $"{alias} (big)", Colors.Black);

                // bottom (small)
                var botRect = new RectF(row.X, row.Y + row.Height * 0.56f, row.Width, row.Height * 0.44f);
                DrawOne(canvas, botRect, alias, smlSize, $"{alias} (small)", Colors.Black);
            }

            canvas.RestoreState();
        }

        /// Draw a single line of centered attributed text into a rectangle.
        private static void DrawOne(ICanvas canvas, RectF bounds, string font, float size, string text, Color color) {
            // Build attributes:
            var attrs = new TextAttributes();

            var randColor = Random.Shared.Next(1, 10) switch {
                1 => Colors.Red,
                2 => Colors.Green,
                3 => Colors.Blue,
                4 => Colors.Yellow,
                5 => Colors.Purple,
                6 => Colors.Orange,
                7 => Colors.Pink,
                8 => Colors.Brown,
                9 => Colors.Gray,
                _ => Colors.Black
            };

            var randSize = Random.Shared.Next(10, 30);

            // Prefer the helpers (MAUI 9):
            attrs.SetFontName("PressStart2P-Regular");
            attrs.SetFontSize(randSize);
            attrs.SetForegroundColor(randColor.ToArgbHex());

            // One run over the whole string:
            var run = new AttributedTextRun(0, text.Length, attrs);
            var attributed = new AttributedText(text, [run]);

            // Draw with the rect overload:
            canvas.FontSize = randSize;
            canvas.DrawText(attributed, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }
    }

    public sealed class PagedFontProbeSkiaView : SKCanvasView {
        private readonly string[] _aliases;
        private readonly int      _start;
        private readonly int      _count;

        public PagedFontProbeSkiaView(string[] aliases, int startIndex = 0, int count = 10) {
            _aliases = aliases ?? System.Array.Empty<string>();
            _start = System.Math.Max(0, startIndex);
            _count = System.Math.Max(1, count);

            EnableTouchEvents = false;
            PaintSurface += OnPaintSurface;
        }

        private void OnPaintSurface(object? sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e) {
            var c = e.Surface.Canvas;
            var w = e.Info.Width;
            var h = e.Info.Height;
            float pad = 12f;

            c.Clear(SKColors.White);

            using var border = new SKPaint { Color = SKColors.LightGray, IsAntialias = true, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
            using var labelP = new SKPaint { Color = SKColors.Gray, IsAntialias = true };
            // Title
            using (var titleFont = new SKFont { Size = 16, Subpixel = true }) {
                c.DrawText($"Skia Font Probe (SkiaSharp 3.x)", w / 2f, pad + 18f, SKTextAlign.Center, titleFont, labelP);
            }

            if (_aliases.Length == 0) {
                using var f = new SKFont { Size = 20, Subpixel = true };
                using var p = new SKPaint { Color = SKColors.Red, IsAntialias = true};
                c.DrawText("No fonts registered", w / 2f, h / 2f, SKTextAlign.Center, f, p);
                return;
            }

            int end = System.Math.Min(_start + _count, _aliases.Length);
            int visible = end - _start;

            float top = pad + 28f;
            float rowH = System.Math.Max(48f, (h - top - pad) / System.Math.Max(1, visible));

            for (int i = _start; i < end; i++) {
                float yTop = top + (i - _start) * rowH;
                var rowR = new SKRect(pad, yTop, w - pad, yTop + rowH - 6f);
                c.DrawRect(rowR, border);

                string alias = _aliases[i];

                // Typeface resolution: filename → family
                var tf = ResolveTypeface(alias);

                // Build modern SKFonts (no TextSize/Typeface on SKPaint)
                using var bigFont = new SKFont(tf, size: System.Math.Min(56f, rowR.Height * 0.50f)) { Subpixel = true };
                using var smallFont = new SKFont(tf, size: System.Math.Min(32f, rowR.Height * 0.30f)) { Subpixel = true };

                using var paint  = new SKPaint { Color = SKColors.Black, IsAntialias = true };

                // vertical centering with metrics (no obsolete MeasureText)
                float CenterBaseline(SKFont font, float rectCenterY) {
                    // Ascent is negative, Descent positive
                    var m = font.Metrics;
                    float half = (m.Descent - m.Ascent) * 0.5f;

                    // To center visually: shift baseline so glyph box is centered on rect center
                    return rectCenterY + half + m.Ascent;
                }

                float cx = rowR.MidX;
                float cy = rowR.MidY;

                // Big line (above center)
                float bigY = CenterBaseline(bigFont, cy - rowH * 0.12f);
                c.DrawText($"{alias} (big)", cx, bigY, SKTextAlign.Center, bigFont, paint);

                // Small line (below center)
                float smlY = CenterBaseline(smallFont, cy + rowH * 0.22f);
                c.DrawText($"{alias} (small)", cx, smlY, SKTextAlign.Center, smallFont, paint);
            }
        }

        private static SKTypeface ResolveTypeface(string alias) {
            // 1) Try to load by filename from your FontCatalog (most reliable)
            try {
                var meta = FontCatalog.GetFontFace(alias); // your helper: alias → { Filename, ... }
                if (meta != null) {
                    using var s = FileSystem.OpenAppPackageFileAsync(meta.Filename).GetAwaiter().GetResult();
                    using var ms = new System.IO.MemoryStream();
                    s.CopyTo(ms);
                    ms.Position = 0;
                    var tf = SKTypeface.FromStream(ms);
                    if (tf != null) return tf;
                }
            } catch { /* ignore and fall back */
            }

            // 2) Family/PostScript match
            return SKFontManager.Default.MatchFamily(alias) ?? SKTypeface.FromFamilyName(alias) ?? SKTypeface.Default;
        }
    }
}