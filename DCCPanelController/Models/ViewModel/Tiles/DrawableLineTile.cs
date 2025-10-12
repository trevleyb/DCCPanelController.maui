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
        gv.SetBinding(ZIndexProperty,  new Binding(nameof(e.Layer),   source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class LineDrawable : IDrawable {
        private readonly LineEntity e;
        private readonly float      w, h;

        public LineDrawable(LineEntity e, float w, float h) { this.e = e; this.w = w; this.h = h; }

        public void Draw(ICanvas canvas, RectF _) {
            canvas.SaveState();
            canvas.Antialias = true;

            var t    = (float)Math.Max(1, e.LineWidth);
            var half = t / 2f;

            var tileW = w;
            var tileH = h;
            var tileWt = tileW - half;
            var tileHt = tileH - half;
            var tileWh = tileW / 2f;
            var tileHh = tileH / 2f;
            var tileWht = tileWh - half;
            var tileHht = tileHh - half;

            (float x1, float y1, float x2, float y2) = e.Rotation switch {
                0   => (-half, tileHht, tileW,  tileHht),
                180 => (tileWht, -half,  tileWht, tileH),
                90  => (-half, -half,  tileWt, tileHt),
                270 => (-half, tileHt, tileWt, -half),
                _   => (-half, tileHht, tileW,  tileHht),
            };

            canvas.StrokeColor = e.LineColor;
            canvas.StrokeSize  = t;
            canvas.DrawLine(x1, y1, x2, y2);
            canvas.RestoreState();
        }
    }
}