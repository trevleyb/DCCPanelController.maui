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
        if (Entity is RectangleEntity entity) {
            Shape shape;
            if (entity.BorderRadius > 0) {
                shape = new RoundRectangle { CornerRadius = entity.BorderRadius };
                shape.SetBinding(RoundRectangle.CornerRadiusProperty, new Binding(nameof(entity.BorderRadius), BindingMode.OneWay, source: entity));
            } else {
                shape = new Rectangle();
            }
            shape.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
            shape.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.OneWay, source: entity));
            shape.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            shape.HorizontalOptions = LayoutOptions.Fill;
            shape.VerticalOptions = LayoutOptions.Fill;
            shape.InputTransparent = true;
            shape.Scale = 1;
            return shape;
        }
        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}