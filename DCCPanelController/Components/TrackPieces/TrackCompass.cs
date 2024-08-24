using System.Runtime.CompilerServices;
using DCCPanelController.Components.TrackPieces.Base;
using DCCPanelController.Components.TrackPieces.SVGManager;

namespace DCCPanelController.Components.TrackPieces;

public class TrackCompass : TrackPiece {
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "Compass", 0);
    }

    public void SetCompassPoints(ITrackPiece track) {
        SetCompassColor("CompassN", track.Connections.ConnectionPointsRotated(track.ImageRotation)[0]);
        SetCompassColor("CompassNE", track.Connections.ConnectionPointsRotated(track.ImageRotation)[1]);
        SetCompassColor("CompassE", track.Connections.ConnectionPointsRotated(track.ImageRotation)[2]);
        SetCompassColor("CompassSE", track.Connections.ConnectionPointsRotated(track.ImageRotation)[3]);
        SetCompassColor("CompassS", track.Connections.ConnectionPointsRotated(track.ImageRotation)[4]);
        SetCompassColor("CompassSW", track.Connections.ConnectionPointsRotated(track.ImageRotation)[5]);
        SetCompassColor("CompassW", track.Connections.ConnectionPointsRotated(track.ImageRotation)[6]);
        SetCompassColor("CompassNW", track.Connections.ConnectionPointsRotated(track.ImageRotation)[7]);
    }

    private void SetCompassColor(string compassId, TrackConnectionsEnum connection) {
        if (ActiveImage == null) return;
        var svgImage = ActiveImage.Value.ImageSource;
        
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