// CompassTile.cs

using System.ComponentModel;
using System.Net.Mime;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Font = Microsoft.Maui.Graphics.Font;

namespace DCCPanelController.Models.ViewModel.Tiles;

public sealed class CompassTile : Tile, ITileDrawable {
    public CompassTile(CompassEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not CompassEntity e)
            throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true,
            Drawable = new CompassDrawable(e)
        };

        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));

        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class CompassDrawable : IDrawable {
        private readonly CompassEntity e;
        public CompassDrawable(CompassEntity entity) => e = entity;

        public void Draw(ICanvas canvas, RectF area) {
            canvas.SaveState();
            canvas.Antialias = true;

            var size = Math.Min(area.Width, area.Height);
            var cx = area.Left + area.Width / 2f;
            var cy = area.Top + area.Height / 2f;
            var radius = size * 0.48f;

            // Face (dial)
            var face = new RectF(cx - radius, cy - radius, radius * 2f, radius * 2f);
            canvas.FillColor = e.BackgroundColor;
            canvas.FillEllipse(face);

            // Border
            var bw = Math.Max(0f, (float)e.BorderWidth);
            if (bw > 0f) {
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize = bw;
                var ring = face.Inflate(new SizeF(-bw / 2f));
                canvas.DrawEllipse(ring);
            }

            // Apply entity rotation (45° steps by RotationFactor)
            if (e.Rotation != 0)
                canvas.Rotate((float)e.Rotation, cx, cy);

            // 8 arrows (N, NE, E, SE, S, SW, W, NW)
            DrawIndicators(canvas, cx, cy, radius);

            // Cardinal labels (moved inward; individually rotated)
            DrawCardinalLabels(canvas, cx, cy, radius);

            // Center decoration
            DrawCentercap(canvas, cx, cy, radius);

            canvas.RestoreState();
        }

        private void DrawIndicators(ICanvas canvas, float cx, float cy, float r) {
            var tipR = r * 0.92f;
            var baseR = r * 0.64f;
            var baseRDiag = r * 0.68f;

            var baseHalfWCard = r * 0.085f;
            var baseHalfWDiag = r * 0.065f;

            canvas.FillColor = e.IndicatorsColor;
            canvas.StrokeColor = e.IndicatorsColor;

            for (int i = 0; i < 8; i++) {
                var deg = i * 45f - 90f; // 0->N
                var rad = (float)(deg * Math.PI / 180.0);
                var isCardinal = (i % 2 == 0);

                var baseRNow = isCardinal ? baseR : baseRDiag;
                var halfWidth = isCardinal ? baseHalfWCard : baseHalfWDiag;

                var tx = cx + (float)Math.Cos(rad) * tipR;
                var ty = cy + (float)Math.Sin(rad) * tipR;

                var bx = cx + (float)Math.Cos(rad) * baseRNow;
                var by = cy + (float)Math.Sin(rad) * baseRNow;

                var pr = rad + (float)Math.PI / 2f;
                var px = (float)Math.Cos(pr) * halfWidth;
                var py = (float)Math.Sin(pr) * halfWidth;

                var p1x = bx + px;
                var p1y = by + py;
                var p2x = bx - px;
                var p2y = by - py;

                using var path = new PathF();
                path.MoveTo(tx, ty);
                path.LineTo(p1x, p1y);
                path.LineTo(p2x, p2y);
                path.Close();

                canvas.FillPath(path);
            }

            // subtle inner accent ring
            canvas.StrokeColor = e.IndicatorsColor.WithAlpha(0.35f);
            canvas.StrokeSize = Math.Max(1f, r * 0.012f);
            canvas.DrawEllipse(cx - r * 0.55f, cy - r * 0.55f, r * 1.10f, r * 1.10f);
        }

        private void DrawCardinalLabels(ICanvas canvas, float cx, float cy, float r) {
            // Move labels inward to avoid arrow bases and the accent ring
            var labelR = r * 0.46f;   // <— was ~0.72r; now well inside arrows
            var fontSize = r * 0.22f; // slightly smaller to fit nicely

            canvas.FontColor = e.DirectionsColor;
            canvas.FontSize = fontSize;
            canvas.Font = new Font("Segoe UI");

            // Positions (after the global entity rotation above)
            DrawRotatedLabel(canvas, "N", cx, cy - labelR, 0f, fontSize, r);
            DrawRotatedLabel(canvas, "E", cx + labelR, cy, 90f, fontSize, r);  // rotate right 90
            DrawRotatedLabel(canvas, "S", cx, cy + labelR, 180f, fontSize, r); // upside down
            DrawRotatedLabel(canvas, "W", cx - labelR, cy, -90f, fontSize, r); // rotate left 90
        }

        private static void DrawRotatedLabel(ICanvas canvas, string text, float x, float y, float angleDeg, float fontSize, float r) {
            // Centered rect around the anchor point; then rotate the canvas about that point.
            var w = r * 0.44f;
            var h = fontSize * 1.2f;
            var rect = new RectF(x - w / 2f, y - h / 2f, w, h);

            canvas.SaveState();
            canvas.Rotate(angleDeg, x, y);
            canvas.DrawString(text, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            canvas.RestoreState();
        }

        private void DrawCentercap(ICanvas canvas, float cx, float cy, float r) {
            canvas.StrokeColor = e.BorderColor.WithAlpha(0.6f);
            canvas.StrokeSize = Math.Max(1f, r * 0.012f);
            canvas.DrawEllipse(cx - r * 0.12f, cy - r * 0.12f, r * 0.24f, r * 0.24f);

            canvas.FillColor = e.IndicatorsColor;
            canvas.FillEllipse(cx - r * 0.05f, cy - r * 0.05f, r * 0.10f, r * 0.10f);
        }
    }
}