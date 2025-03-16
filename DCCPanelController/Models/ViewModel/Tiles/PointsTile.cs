using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class PointsTile : Tile {
    public PointsTile(PointsEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {

        if (Entity is PointsEntity entity) {
            var svgImage = SvgImages.GetImage("points", Entity.Rotation);
            var image = new Image {
                Scale = 1.5
            };
            image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), BindingMode.OneWay, source: svgImage));
            return image;
        } 
        return CreateSymbol();
    }
    
    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return SvgImages.GetImage("points").AsImage();
    }

    
    public void SetPoints(SvgImage svgImage, bool[] points) {
        for (var point = 0; point < 8; point++) {
            SetPointColor(svgImage, PointLabel(point), points[point]);
        }
    }

    private bool SetPointColor(SvgImage svgImage, string elementID, bool isValid) {
        if (isValid) {
            svgImage.ApplyElementStyle(elementID, "Color", Colors.Transparent.ToRgbaHex());
            svgImage.ApplyElementStyle(elementID, "Opacity", "0");
        } else {
            svgImage.ApplyElementStyle(elementID, "Color", Colors.Red.ToRgbaHex());
            svgImage.ApplyElementStyle(elementID, "Opacity", "50");
        }
        return isValid;
    }

    private string PointLabel(int direction) {
        return direction switch {
            0 => "PointN",
            1 => "PointNE",
            2 => "PointE",
            3 => "PointSE",
            4 => "PointS",
            5 => "PointSW",
            6 => "PointW",
            7 => "PointNW",
            _ => "PointN"
        };
    }
}