using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class LineTile : Tile {
    public LineTile(LineEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(entity.LineColor));
        VisualProperties.Add(nameof(entity.LineWidth));
        VisualProperties.Add(nameof(entity.Opacity));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is LineEntity entity) {
            var line = new Line() {
                X1 = 0,
                Y1 = 0,
                X2 = TileWidth,
                Y2 = TileHeight,
                Stroke = entity.LineColor,
                StrokeThickness = entity.LineWidth,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                ZIndex = entity.Layer,
                Opacity = Opacity,
                InputTransparent = true
            };
            line.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: this));
            return line;
        } 
        return CreateSymbol();
    }   
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("line").AsImage();
    }

}