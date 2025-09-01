using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Views.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableCircleTile : Tile, ITileDrawable {
    public DrawableCircleTile(CircleEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(CircleEntity.BackgroundColor));
        VisualProperties.Add(nameof(CircleEntity.BorderColor));
        VisualProperties.Add(nameof(CircleEntity.BorderWidth));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is CircleEntity entity) {
            var circle = new Ellipse {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                InputTransparent = true
            };
            circle.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
            circle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));
            circle.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: entity));
            return circle;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("circle").AsImage();
    }
}