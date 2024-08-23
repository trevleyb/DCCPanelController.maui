using System.Runtime.CompilerServices;
using DCCPanelController.Components.SVGManager;
using DCCPanelController.Components.TrackPieces.Base;

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
            svgImage.SetElementAttribute(compassId, "Color", Colors.Yellow.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Straight:
            svgImage.SetElementAttribute(compassId, "Color", Colors.Blue.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Closed:
            svgImage.SetElementAttribute(compassId, "Color", Colors.Green.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Diverging:
            svgImage.SetElementAttribute(compassId, "Color", Colors.Red.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.Connector:
            svgImage.SetElementAttribute(compassId, "Color", Colors.Magenta.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "100");
            break;
        case TrackConnectionsEnum.None:
        default:
            svgImage.SetElementAttribute(compassId, "Color", Colors.Transparent.ToRgbaHex());
            svgImage.SetElementAttribute(compassId, "Opacity", "0");
            break;
        }
    }
}