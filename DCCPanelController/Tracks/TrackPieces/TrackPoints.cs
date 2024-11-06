using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks;

public class TrackPoints : TrackPiece {
    protected override void Setup() {
        Name = "Track Points";
        DefaultState = "Normal";
        SetTrackSymbol("Points");
        Layer = 2;
    }

    protected override void AddTrackImages() {
        AddTrackImage(0, "Normal", "Points", 0);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal", "Mainline");
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
            7 => "PointNW"
        };
    }
}