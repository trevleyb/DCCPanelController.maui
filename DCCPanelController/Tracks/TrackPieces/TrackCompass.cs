using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackCompass : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        Layer = 2;
        Name = "Compass";
        SetTrackSymbol("Compass");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Compass", (0, 0), (90, 0), (180, 0), (270, 0));
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
    
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}