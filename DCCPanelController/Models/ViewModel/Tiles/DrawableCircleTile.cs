using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.TileCache;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleTile : Tile, ITileDrawable {
    public DrawableCircleTile(CircleEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is not CircleEntity e) throw new TileRenderException(GetType(), Entity.GetType());

        var gv = new GraphicsView {
            WidthRequest = TileWidth,
            HeightRequest = TileHeight,
            InputTransparent = true,
            Drawable = new CircleDrawable(e)
        };
        gv.SetBinding(ScaleProperty, new Binding(nameof(e.Scale), source: e));
        gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
        gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));

        e.PropertyChanged += (_, __) => gv.Invalidate(); // redraw on visual changes
        return gv;
    }

    private sealed class CircleDrawable : IDrawable {
        private readonly CircleEntity e;
        public CircleDrawable(CircleEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF r) {
            
            canvas.SaveState();
            canvas.Antialias = true;

            if (e.BackgroundColor is { } bg) {
                canvas.FillColor = bg;
                canvas.FillEllipse(r);
            }

            // Stroke (inset by half-thickness so stroke stays inside the tile)
            var t = (float)Math.Max(0, e.BorderWidth);
            if (t > 0f) {
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize = t;
                var inset = r.Inflate(new SizeF(-t / 2f));
                canvas.DrawEllipse(inset);
            }
            canvas.RestoreState();
        }
    }
}