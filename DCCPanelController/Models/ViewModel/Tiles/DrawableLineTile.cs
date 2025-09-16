using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class DrawableLineTile : Tile, ITileDrawable {
    public DrawableLineTile(LineEntity entity, double gridSize) : base(entity, gridSize) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        if (Entity is LineEntity entity) {
            var thickness = Math.Max(1.0, entity.LineWidth); // guard
            var half = thickness / 2.0;
            var tileW = TileWidth;       // Tile Width as Double
            var tileH = TileHeight;      // Tile Hight as Double
            var tileWt = tileW - half;   // Adjust for the width of the line
            var tileHt = tileH - half;   // Adjust for the width of the line
            var tileWh = tileW / 2;      // Half the Tile Width - Center
            var tileHh = tileH / 2;      // Half the Tile Height - Center
            var tileWht = tileWh - half; // Adjust for the width of the line
            var tileHht = tileHh - half; // Adjust for the width of the line

            var (x1, y1, x2, y2) = entity.Rotation switch {
                0   => (-half, tileHht, tileW, tileHht), // horizontal centered
                180 => (tileWht, -half, tileWht, tileH), // vertical centered
                90  => (-half, -half, tileWt, tileHt),   // TL -> BR diagonal
                270 => (-half, tileHt, tileWt, -half),   // BL -> TR diagonal
                _   => (-half, tileHht, tileW, tileHht),
            };

            var shape = new Line {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                InputTransparent = true,
            };
            shape.SetBinding(Shape.StrokeProperty, new Binding(nameof(entity.LineColor), BindingMode.OneWay, new ColorToSolidColorConverter(), source: entity));
            shape.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(entity.LineWidth), BindingMode.OneWay, source: entity));
            shape.SetBinding(OpacityProperty, new Binding(nameof(entity.Opacity), BindingMode.OneWay, source: entity));
            shape.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.OneWay, source: entity));
            return shape;
        }
        throw new TileRenderException(this.GetType(), Entity.GetType());
    }
}