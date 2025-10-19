using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableLineTile : Tile, ITileDrawable {
    public DrawableLineTile(LineEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not LineEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new LineDrawable(e, (float)TileWidth, (float)TileHeight)
        };

        gv.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class LineDrawable : IDrawable {
        private readonly LineEntity e;
        private readonly float      w, h;

        public LineDrawable(LineEntity e, float w, float h) {
            this.e = e;
            this.w = w;
            this.h = h;
        }

        public void Draw(ICanvas canvas, RectF _) {
            canvas.SaveState();
            canvas.Antialias = true;

            float stroke = (float)Math.Max(1, e.LineWidth);
            float half = stroke / 2f;

            float w2 = w / 2f, h2 = h / 2f;
            float cx = w2, cy = h2;

            float xmin = half, xmax = w - half;
            float ymin = half, ymax = h - half;

            float theta = (float)(Math.PI / 180.0) * (float)(e.Rotation % 360);
            float dx = MathF.Cos(theta);
            float dy = MathF.Sin(theta);

            var hits = new List<(float t, float x, float y)>(4);

            void TryHit(bool vertical, float bound) {
                if (vertical) {
                    if (MathF.Abs(dx) < 1e-6f) return;
                    float tt = (bound - cx) / dx;
                    float yy = cy + tt * dy;
                    if (yy >= ymin - 1e-3f && yy <= ymax + 1e-3f)
                        hits.Add((tt, bound, yy));
                } else {
                    if (MathF.Abs(dy) < 1e-6f) return;
                    float tt = (bound - cy) / dy;
                    float xx = cx + tt * dx;
                    if (xx >= xmin - 1e-3f && xx <= xmax + 1e-3f)
                        hits.Add((tt, xx, bound));
                }
            }

            TryHit(true, xmin);  // left
            TryHit(true, xmax);  // right
            TryHit(false, ymin); // top
            TryHit(false, ymax); // bottom

            float x1, y1, x2, y2;

            if (hits.Count < 2) {
                // Fallback: draw inscribed line within circle
                float r = MathF.Min(w2, h2) - half;
                x1 = cx - dx * r;
                y1 = cy - dy * r;
                x2 = cx + dx * r;
                y2 = cy + dy * r;
            } else {
                var min = hits.MinBy(h => h.t);
                var max = hits.MaxBy(h => h.t);
                x1 = min.x;
                y1 = min.y;
                x2 = max.x;
                y2 = max.y;
            }

            canvas.StrokeColor = e.LineColor;
            canvas.StrokeSize = stroke;
            canvas.StrokeLineCap = LineCap.Butt;
            canvas.DrawLine(x1, y1, x2, y2);

            canvas.RestoreState();
        }

        public void DrawFixed4(ICanvas canvas, RectF _) {
            canvas.SaveState();
            canvas.Antialias = true;

            var t = (float)Math.Max(1, e.LineWidth);
            var half = t / 2f;

            var tileW = w;
            var tileH = h;
            var tileWt = tileW - half;
            var tileHt = tileH - half;
            var tileWh = tileW / 2f;
            var tileHh = tileH / 2f;
            var tileWht = tileWh - half;
            var tileHht = tileHh - half;
            var centerY = tileH / 2f;

            (float x1, float y1, float x2, float y2) = e.Rotation switch {
                0   => (-half, centerY, tileW + half, centerY), // perfectly centered horizontal line
                180 => (centerY, -half, centerY, tileH + half),
                90  => (-half, -half, tileWt, tileHt),
                270 => (-half, tileHt, tileWt, -half),
                _   => (-half, centerY, tileW + half, centerY),
            };

            canvas.StrokeColor = e.LineColor;
            canvas.StrokeSize = t;
            canvas.DrawLine(x1, y1, x2, y2);
            canvas.RestoreState();
        }
    }
}