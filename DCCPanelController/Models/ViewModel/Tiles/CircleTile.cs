using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CircleTile : Tile {
    public CircleTile(CircleEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is CircleEntity entity) {
            var circle = new Ellipse {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                InputTransparent = true,
            };
            circle.SetBinding(Shape.OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
            circle.SetBinding(Shape.FillProperty, new Binding(nameof(entity.BackgroundColor), BindingMode.TwoWay, converter: new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.BorderColor), BindingMode.TwoWay, converter: new ColorToSolidColorConverter(), source: entity));
            circle.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.BorderWidth), BindingMode.TwoWay, source: entity));
            return circle;
        } 
        return CreateSymbol();
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("circle").AsImage();
    }
}