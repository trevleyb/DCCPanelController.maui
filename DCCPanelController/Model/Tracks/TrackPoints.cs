using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.Tracks;

public class TrackPoints(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPiece(parent, styleTypeEnum), ITrack {
    public TrackPoints() : this(null) { }

    public string Name => "Connection Points";

    public ITrack Clone(Panel parent) {
        return Clone<TrackPoints>(parent);
    }

    [property: EditableInt(Name = "Layer", Group = "Attributes", Description = "What Layer does this peice sit on?", MinValue = 1, MaxValue = 5, Order = 5)]
    public new int Layer {
        get => base.Layer;
        set => base.Layer = value;
    }
    
    protected override void Setup() {
        Layer = 5;
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