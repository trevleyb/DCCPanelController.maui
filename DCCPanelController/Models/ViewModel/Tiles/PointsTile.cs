using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class PointsTile : Tile {
    
    public PointsTile(TrackEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTileAsCanvas();
    }

    protected Microsoft.Maui.Controls.View? CreateTrackTileAsCanvas() {
        var svgImage = SvgImages.GetImage("points",0);

        var canvas = svgImage.AsCanvas(svgImage.Rotation, 1);
        canvas.HorizontalOptions = LayoutOptions.Fill;
        canvas.VerticalOptions = LayoutOptions.Fill;
        canvas.Opacity = 0.5;

        ColorPoints(svgImage);
        
        var absoluteLayout = new AbsoluteLayout();
        AbsoluteLayout.SetLayoutBounds(canvas, new Rect(-GridSize * 0.25, -GridSize * 0.25, GridSize * 1.5, GridSize * 1.5));
        absoluteLayout.Children.Add(canvas);
        return absoluteLayout;
    }
    
    protected Microsoft.Maui.Controls.View? CreateTileAsImage() {
        // if (Entity is PointsEntity entity) {
        //     var svgImage = SvgImages.GetImage("points", Entity.Rotation);
        //     var image = new Image {
        //         Scale = 1.5,
        //         Source = svgImage.AsImageSource(0, DefaultScaleFactor),
        //     };
        //     image.SetBinding(RotationProperty, new Binding(nameof(Rotation), BindingMode.OneWay, source: svgImage));
        //     image.SetBinding(ZIndexProperty, new Binding(nameof(entity.Layer), BindingMode.TwoWay, source: entity));
        //     return image;
        // }
        return CreateSymbol();
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => SvgImages.GetImage("points").AsImage();

    private void ColorPoints(SvgImage svgImage) {
        if (Entity is TrackEntity entity) {
            for (var i = 0; i < 8; i++) {

                // Get the direction indicator (*, T, X, D, S)
                var direction = entity.GetCurrentConnections[i];

                Color? color = direction switch {
                    ConnectionType.None       => null,
                    ConnectionType.Terminator => Colors.Black,
                    ConnectionType.Straight   => Colors.Green,
                    ConnectionType.Closed     => Colors.BlueViolet,
                    ConnectionType.Diverging  => Colors.Red,
                    ConnectionType.Connector  => Colors.Blue,
                    _                         => null
                };

                if (color is { }) {
                    svgImage.ApplyElementStyle(PointLabel(i), "Color", color.ToRgbaHex());
                    svgImage.ApplyElementStyle(PointLabel(i), "Opacity", "50");
                } else {
                    svgImage.ApplyElementStyle(PointLabel(i), "Opacity", "0");
                }
            }
        }
    }
    
    private string PointLabel(int direction) => direction switch {
        0 => "PointN",
        1 => "PointNE",
        2 => "PointE",
        3 => "PointSE",
        4 => "PointS",
        5 => "PointSW",
        6 => "PointW",
        7 => "PointNW",
        _ => "PointN",
    };
}