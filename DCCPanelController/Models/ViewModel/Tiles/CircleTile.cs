using DCCPanelController.Models.DataModel.Entities;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CircleTile : Tile{
    public CircleTile(Entity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is CircleEntity entity) {
            var circle = new Ellipse {
                Fill = entity.BackgroundColor,
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
        return null;
    }
}