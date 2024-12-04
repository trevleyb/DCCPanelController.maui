using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackPoints : TrackPieceBase, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Track Points")]
    private string _name = "Points";
    
    protected override void Setup() {
        Layer = 2;
        SetTrackSymbol("Points");
        AddImageSourceAndRotation(TrackStyleImage.Normal,  "Points", (0, 0), (90 ,90), (180 ,180), (270, 270));
    }
    
    public void SetPoints(bool[] points) {
        var svgImage = ActiveImage;
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