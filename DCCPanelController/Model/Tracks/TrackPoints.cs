using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackPoints(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackPiece {
    public TrackPoints() : this(null) { }

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackPoints>(parent);
    }

    protected override void Setup() {
        Layer = 2;
        Name = "Points";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Points", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Points", (0, 0), (90, 90), (180, 180), (270, 270));
    }

    public void SetPoints(bool[] points) {
        if (ActiveImage is { } svgImage) {
            for (var point = 0; point < 8; point++) {
                SetPointColor(svgImage, PointLabel(point), points[point]);
            }
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