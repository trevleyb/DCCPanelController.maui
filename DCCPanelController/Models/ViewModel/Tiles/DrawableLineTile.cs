using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableLineTile : Tile, ITileDrawable {
    public DrawableLineTile(LineEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) {
        VisualProperties.Add(nameof(LineEntity.LineColor));
        VisualProperties.Add(nameof(LineEntity.LineWidth));
    }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is LineEntity entity) {
            var shape = new Line {
                X1 = 0,
                Y1 = TileHeight /2,
                X2 = TileWidth,
                Y2 = TileHeight /2,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                InputTransparent = true
            };
            shape.SetBinding(RotationProperty, new Binding(nameof(entity.Rotation), BindingMode.OneWay, source: entity));
            shape.SetBinding(Line.X2Property, new Binding(nameof(TileWidth), BindingMode.OneWay, source: this));
            shape.SetBinding(Line.Y2Property, new Binding(nameof(TileHeight), BindingMode.OneWay, source: this));
            shape.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
            shape.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.LineColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.LineWidth), BindingMode.OneWay, source: entity));
            shape.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            return shape;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("line").AsImage();
    }
}