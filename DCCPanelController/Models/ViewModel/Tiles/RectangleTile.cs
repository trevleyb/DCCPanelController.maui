using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class RectangleTile : Tile {
    public RectangleTile(RectangleEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is RectangleEntity entity) {
            var square = new Rectangle() {
                Fill = entity.BackgroundColor,
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
            square.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return square;
        } 
        return CreateSymbol();
    }   
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("rectangle").AsImage();
    }
}