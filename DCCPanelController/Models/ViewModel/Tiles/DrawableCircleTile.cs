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
        if (Entity is CircleEntity entity) {
            var circle = new Ellipse {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                InputTransparent = true,
            };
            circle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
            circle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.OneWay, source: entity));
            circle.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            return circle;
        }
        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}