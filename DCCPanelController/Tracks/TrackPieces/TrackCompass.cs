using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackCompass : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "A Compass")]
    private string _name = "Compass";
    
    protected override void Setup() {
        Layer = 2;
        SetTrackSymbol("Compass");
        AddImageSourceAndRotation(TrackStyleImage.Unknown, "Compass", (0, 0), (90 ,0), (180 ,0), (270, 0));
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