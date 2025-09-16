using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleTile : Tile, ITileDrawable {
    public DrawableCircleTile(CircleEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        using (new CodeTimer("Draw Circle Tile")) {
            if (Entity is not CircleEntity e) throw new TileRenderException(GetType(), Entity.GetType());

            var gv = new GraphicsView {
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                InputTransparent = true,
                Drawable = new CircleDrawable(e)
            };
            gv.SetBinding(ZIndexProperty, new Binding(nameof(e.Layer), source: e));
            gv.SetBinding(OpacityProperty, new Binding(nameof(e.Opacity), source: e));

            e.PropertyChanged += (_, __) => gv.Invalidate(); // redraw on visual changes
            return gv;
        }
    }

    private sealed class CircleDrawable : IDrawable {
        private readonly CircleEntity e;
        public CircleDrawable(CircleEntity e) => this.e = e;

        public void Draw(ICanvas canvas, RectF dirty) {
            canvas.SaveState();
            canvas.Antialias = true;

            if (e.BackgroundColor is { } bg) {
                canvas.FillColor = bg;
                canvas.FillEllipse(dirty);
            }

            // Stroke (inset by half-thickness so stroke stays inside the tile)
            var t = (float)Math.Max(0, e.BorderWidth);
            if (t > 0f) {
                canvas.StrokeColor = e.BorderColor;
                canvas.StrokeSize  = t;
                var inset = dirty.Inflate(new SizeF(-t / 2f));
                canvas.DrawEllipse(inset);
            }
            canvas.RestoreState();
        }
    }

    
    // protected override Microsoft.Maui.Controls.View? CreateTile() {
    //     using (new CodeTimer("Draw Circle Maui")) {
    //         if (Entity is CircleEntity entity) {
    //             var circle = new Ellipse {
    //                 HorizontalOptions = LayoutOptions.Start,
    //                 VerticalOptions = LayoutOptions.Start,
    //                 InputTransparent = true,
    //             };
    //             circle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
    //             circle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
    //             circle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
    //             circle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.OneWay, source: entity));
    //             circle.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
    //             return circle;
    //         }
    //         throw new TileRenderException(this.GetType(), Entity.GetType());
    //     }
    // }
}