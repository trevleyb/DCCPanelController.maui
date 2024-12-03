using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

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
        if (ActiveImage == null || points.Length != 8) {
            ActiveImage = null;
            return;
        }

        var svgImage = ActiveImage.Value.ImageSource;
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