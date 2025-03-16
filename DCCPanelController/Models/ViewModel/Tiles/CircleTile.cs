using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CircleTile : Tile {
    public CircleTile(CircleEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is CircleEntity entity) {
            var circle = new Ellipse {
                Fill = entity.BackgroundColor ?? Colors.Black,
                Stroke = entity.BorderColor,
                StrokeThickness = entity.BorderWidth,
                WidthRequest = TileWidth,
                HeightRequest = TileHeight,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = entity.Layer,
                Opacity = entity.Opacity,
                InputTransparent = true,
                Scale = 1
            };
            circle.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return circle;
        } 
        return CreateSymbol();
    }
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("circle").AsImage();
    }
}