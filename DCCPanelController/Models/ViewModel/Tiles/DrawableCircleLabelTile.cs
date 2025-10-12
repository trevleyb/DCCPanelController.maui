using System.Runtime.InteropServices;
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
    public DrawableCircleLabelTile(CircleLabelEntity entity, double gridSize) : base(entity, gridSize) {
        Watch
            .Track(nameof(CircleLabelEntity.BackgroundColor), () => entity.BackgroundColor)
            .Track(nameof(CircleLabelEntity.BorderOuterColor), () => entity.BorderOuterColor)
            .Track(nameof(CircleLabelEntity.BorderInnerColor), () => entity.BorderInnerColor)
            .Track(nameof(CircleLabelEntity.GapColor), () => entity.GapColor)
            .Track(nameof(CircleLabelEntity.BorderOuterWidth), () => entity.BorderOuterWidth)
            .Track(nameof(CircleLabelEntity.BorderInnerWidth), () => entity.BorderInnerWidth)
            .Track(nameof(CircleLabelEntity.GapWidth), () => entity.GapWidth)
            .Track(nameof(CircleLabelEntity.FontSize), () => entity.FontSize)
            .Track(nameof(CircleLabelEntity.FontAlias), () => entity.FontAlias)
            .Track(nameof(CircleLabelEntity.Label), () => entity.Label)
            .Track(nameof(CircleLabelEntity.TextColor), () => entity.TextColor);
    }

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

        public CircleLabelDrawable(CircleLabelEntity e) {
            this.e = e;
        }

        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            // --- Inputs (clamped) ---
            float outerStroke = (float)Math.Max(0, e.BorderOuterWidth);
            float gapStroke = (float)Math.Max(0, e.GapWidth);
            float innerStroke = (float)Math.Max(0, e.BorderInnerWidth);

            // --- 1) OUTER BORDER ---
            // Draw stroke so it stays fully inside the tile.
            if (outerStroke > 0f) {
                canvas.StrokeColor = e.BorderOuterColor;
                canvas.StrokeSize = outerStroke;
                var outerRect = r.Inflate(new SizeF(-outerStroke / 2f));
                canvas.DrawEllipse(outerRect);
            }

            // --- 2) GAP RING ---
            // "Edge of the outside of the gap border" == "edge of the inside of the outer border".
            // With centered stroke, that means inset by outerStroke + gapStroke/2.
            if (gapStroke > 0f) {
                canvas.StrokeColor = e.GapColor;
                canvas.StrokeSize = gapStroke;
                var gapRect = r.Inflate(new SizeF(-(outerStroke + gapStroke / 2f)));
                canvas.DrawEllipse(gapRect);
            }

            // --- 3) INNER CIRCLE (fill) + INNER BORDER ---
            // The inner circle boundary is the inner edge of the gap: inset by (outerStroke + gapStroke).
            var innerCircleRect = r.Inflate(new SizeF(-(outerStroke + gapStroke)));

            // Fill
            if (e.BackgroundColor is { } fill) {
                canvas.FillColor = fill;
                canvas.FillEllipse(innerCircleRect);
            }

            // Inner border: keep the entire stroke inside the inner circle, so inset by innerStroke/2.
            if (innerStroke > 0f) {
                canvas.StrokeColor = e.BorderInnerColor;
                canvas.StrokeSize = innerStroke;
                var innerBorderRect = innerCircleRect.Inflate(new SizeF(-innerStroke / 2f));
                canvas.DrawEllipse(innerBorderRect);
            }

            // --- 4) TEXT inside the INNER CIRCLE only ---
            // Use inner content area that avoids the inner border thickness.
            var innerContentRect = innerCircleRect.Inflate(new SizeF(-innerStroke)); // keep clear of the inner border

            // --- 5) TEXT (fit inside inner circle; scale down if needed; CENTERED baseline) -------
            // --- TEXT: fit inside inner circle, 2pt margin, rotate by Entity.Rotation (SkiaSharp 3.119) ---
            if (!string.IsNullOrWhiteSpace(e.Label)) {
                // 1) Fixed 2pt padding from the inner circle edge
                const float textPadding = 2f;
                var textRect = innerContentRect.Inflate(new SizeF(-textPadding));

                // Clip to inner ellipse so nothing bleeds outside
                var clip = new PathF();
                clip.AppendEllipse(innerContentRect);
                canvas.ClipPath(clip);

                // Typeface + font/paint for SkiaSharp 3.x
                var typeface = ResolveTypeface(e.FontAlias);
                float pointSize = FontSizeHelper.UnitToSize(e.FontSize <= 0 ? 14f : e.FontSize);

                using var skFont = new SkiaSharp.SKFont(typeface, pointSize) { Subpixel = true, LinearMetrics = true };
                using var skPaint = new SkiaSharp.SKPaint { IsAntialias = true, Color = ToSkColor(e.TextColor) };

                // Build glyph path at origin (0,0) — SkiaSharp 3.119 API
                using var textPath = skFont.GetTextPath(e.Label, new SkiaSharp.SKPoint(0, 0));

                // Tight bounds of the actual outlines
                var b = textPath.TightBounds;
                float w = b.Width;
                float h = b.Height;

                // Scale DOWN only to keep outline inside textRect
                float sx = (w > 0f) ? textRect.Width / w : 1f;
                float sy = (h > 0f) ? textRect.Height / h : 1f;
                float s = MathF.Min(1f, MathF.Min(sx, sy));

                // Center the path bounds inside textRect
                float cx = textRect.Center.X;
                float cy = textRect.Center.Y;

                using var recorder = new SkiaSharp.SKPictureRecorder();
                var picRect = new SkiaSharp.SKRect(0, 0, textRect.Width, textRect.Height);
                var skCanvas = recorder.BeginRecording(picRect);

                // Move to textRect space
                skCanvas.Translate(-textRect.Left, -textRect.Top);

                // 2) Rotate around the inner circle center (Entity.Rotation in degrees)
                float rotationDeg = (float)e.Rotation; // assuming e.Rotation is degrees
                skCanvas.Translate(cx, cy);
                skCanvas.RotateDegrees(rotationDeg);

                // keep text centered while scaling
                skCanvas.Scale(s, s);
                skCanvas.Translate(-cx, -cy);

                // Translate so the path-bounds center sits at (cx, cy)
                float bxCenter = b.Left + w / 2f;
                float byCenter = b.Top + h / 2f;
                skCanvas.Translate(cx - bxCenter, cy - byCenter);

                // Draw the outlines (filled text: set skPaint.Style = Fill; stroke text: Style = Stroke)
                skPaint.Style = SkiaSharp.SKPaintStyle.Fill;
                skCanvas.DrawPath(textPath, skPaint);

                using var picture = recorder.EndRecording();
                using var skImage = SkiaSharp.SKImage.FromPicture(
                    picture,
                    new SkiaSharp.SKSizeI(
                        (int)MathF.Ceiling(textRect.Width),
                        (int)MathF.Ceiling(textRect.Height))
                );

                using var skData = skImage.Encode();

                var platformImage = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(skData.AsStream());
                canvas.DrawImage(platformImage, textRect.X, textRect.Y, textRect.Width, textRect.Height);
            }

            canvas.RestoreState();
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
}