using System.Security.Cryptography;
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
            
            double thickness = Math.Max(1.0, entity.LineWidth); // guard
            double half = thickness / 2.0;
            double tileW = TileWidth;       // Tile Width as Double
            double tileH = TileHeight;      // Tile Hight as Double
            double tileWt = tileW - half;   // Adjust for the width of the line
            double tileHt = tileH - half;   // Adjust for the width of the line
            double tileWh = tileW / 2;      // Half the Tile Width - Center
            double tileHh = tileH / 2;      // Half the Tile Height - Center
            double tileWht = tileWh - half; // Adjust for the width of the line
            double tileHht = tileHh - half; // Adjust for the width of the line

            var (x1, y1, x2, y2) = entity.Rotation switch
            {
                0   => (-half,      tileHht,    tileW,      tileHht),        // horizontal centered
                180 => (tileWht,    -half,      tileWht,    tileH),         // vertical centered
                90  => (-half,   -half,   tileWt,  tileHt), // TL -> BR diagonal
                270 => (-half, tileHt,   tileWt,  -half),   // BL -> TR diagonal
                _   => (-half,      tileHht,    tileW,      tileHht)
            };

            var shape = new Line {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                InputTransparent = true
            };
            shape.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.LineColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.LineWidth), BindingMode.OneWay, source: entity));
            shape.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
            shape.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            // shape.SetBinding(Line.X2Property, new Binding(nameof(TileWidth), BindingMode.OneWay, source: this));
            // shape.SetBinding(Line.Y2Property, new Binding(nameof(TileHeight), BindingMode.OneWay, source: this));
            return shape;
        }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("line").AsImage();
    }
}