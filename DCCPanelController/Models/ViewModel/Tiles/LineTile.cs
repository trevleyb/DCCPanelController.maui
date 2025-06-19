using DCCPanelController.Helpers.Converters;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class LineTile : Tile {
    public LineTile(LineEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(LineEntity.LineColor));
        VisualProperties.Add(nameof(LineEntity.LineWidth));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is LineEntity entity) {
            var shape = new Line {
                X1 = 0,
                Y1 = 0,
                X2 = TileWidth,
                Y2 = TileHeight,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                InputTransparent = true
            };
            shape.SetBinding(RotationProperty, new Binding(nameof(entity.Rotation), BindingMode.TwoWay, source: entity));
            shape.SetBinding(Line.X2Property, new Binding(nameof(TileWidth), BindingMode.TwoWay, source: this));
            shape.SetBinding(Line.Y2Property, new Binding(nameof(TileHeight), BindingMode.TwoWay, source: this));
            shape.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.TwoWay, source: entity));
            shape.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.LineColor), BindingMode.TwoWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.LineWidth), BindingMode.TwoWay, source: entity));
            shape.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: entity));
            return shape;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("line").AsImage();
    }
}