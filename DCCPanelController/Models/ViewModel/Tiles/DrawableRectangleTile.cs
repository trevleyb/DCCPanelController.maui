using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableRectangleTile : Tile, ITileDrawable {
    public DrawableRectangleTile(RectangleEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not RectangleEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new RectDrawable(e)
        };
        gv.SetBinding(ZIndexProperty,  new Binding(nameof(e.Layer),   source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));
        e.PropertyChanged += (_, __) => gv.Invalidate();
        return gv;
    }

    private sealed class RectDrawable : IDrawable {
        private readonly RectangleEntity e;
        public RectDrawable(RectangleEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF r) {
            canvas.SaveState();
            canvas.Antialias = true;

            var radius = (float)Math.Max(0, e.BorderRadius);
            var t      = (float)Math.Max(0, e.BorderWidth);

            if (e.BackgroundColor is { } fill) {
                canvas.FillColor = fill;
                if (radius > 0) canvas.FillRoundedRectangle(r, radius);
                else            canvas.FillRectangle(r);
            }

            // Stroke (inset for stroke-on-inside look)
            if (t > 0f) {
                var inset = r.Inflate(new SizeF(-t / 2f));
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize  = t;
                if (radius > 0) canvas.DrawRoundedRectangle(inset, Math.Max(0, radius - t / 2f));
                else            canvas.DrawRectangle(inset);
            }
            canvas.RestoreState();
        }
    }
}