using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class RectangleTile : Tile {
    public RectangleTile(RectangleEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(entity.BackgroundColor));
        VisualProperties.Add(nameof(entity.BorderColor));
        VisualProperties.Add(nameof(entity.BorderWidth));
        VisualProperties.Add(nameof(entity.BorderRadius));
        VisualProperties.Add(nameof(entity.Opacity));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is RectangleEntity entity) {

            Shape shape;
            if (entity.BorderRadius > 0) {
                shape = new RoundRectangle() {
                    CornerRadius = entity.BorderRadius,
                };
            } else {
                shape = new Rectangle();
            }
            shape.Fill = entity.BackgroundColor;
            shape.Stroke = entity.BorderColor;
            shape.StrokeThickness = entity.BorderWidth;
            shape.WidthRequest = TileWidth;
            shape.HeightRequest = TileHeight;
            shape.HorizontalOptions = LayoutOptions.Start;
            shape.VerticalOptions = LayoutOptions.Start;
            shape.ZIndex = entity.Layer;
            shape.Opacity = entity.Opacity;
            shape.InputTransparent = true;
            shape.Scale = 1;
            shape.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return shape;
        } 
        return CreateSymbol();
    }   
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("rectangle").AsImage();
    }
}