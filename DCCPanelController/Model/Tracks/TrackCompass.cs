using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackCompass(Panel? parent = null) : TrackPiece(parent), ITrackSymbol, ITrack {
    public TrackCompass() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackCompass>(parent);
    }

    public string Name => "Compass";

    protected override void Setup() {
        Layer = 5;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Compass", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Compass", (0, 0), (90, 0), (180, 0), (270, 0));
    }

    public void SetCompassPoints(ITrack track) {
        SetCompassColor("CompassN", track.Connection(0));
        SetCompassColor("CompassNE", track.Connection(1));
        SetCompassColor("CompassE", track.Connection(2));
        SetCompassColor("CompassSE", track.Connection(3));
        SetCompassColor("CompassS", track.Connection(4));
        SetCompassColor("CompassSW", track.Connection(5));
        SetCompassColor("CompassW", track.Connection(6));
        SetCompassColor("CompassNW", track.Connection(7));
    }

    private void SetCompassColor(string compassId, TrackConnectionsEnum connection) {
        if (ActiveImage == null) return;
        var svgImage = ActiveImage;

        switch (connection) {
        case TrackConnectionsEnum.Terminator:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Yellow.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "100");
            break;

        case TrackConnectionsEnum.Straight:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Blue.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "100");
            break;

        case TrackConnectionsEnum.Closed:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Green.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "100");
            break;

        case TrackConnectionsEnum.Diverging:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Red.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "100");
            break;

        case TrackConnectionsEnum.Connector:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Magenta.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "100");
            break;

        case TrackConnectionsEnum.None:
        default:
            svgImage.ApplyElementStyle(compassId, "Color", Colors.Transparent.ToRgbaHex());
            svgImage.ApplyElementStyle(compassId, "Opacity", "0");
            break;
        }
    }
}