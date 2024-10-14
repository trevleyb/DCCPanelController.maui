using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackCompass : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Compass";
        DefaultState = "Normal";
        SetTrackSymbol("Compass");
        Layer = 2;
    }

    protected override void AddTrackImages() {
        AddTrackImage(0, "Normal", "Compass", 0);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }
    
    public void SetCompassPoints(ITrackPiece track) {
        SetCompassColor("CompassN", track.Connections[0]);
        SetCompassColor("CompassNE", track.Connections[1]);
        SetCompassColor("CompassE", track.Connections[2]);
        SetCompassColor("CompassSE", track.Connections[3]);
        SetCompassColor("CompassS", track.Connections[4]);
        SetCompassColor("CompassSW", track.Connections[5]);
        SetCompassColor("CompassW", track.Connections[6]);
        SetCompassColor("CompassNW", track.Connections[7]);
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