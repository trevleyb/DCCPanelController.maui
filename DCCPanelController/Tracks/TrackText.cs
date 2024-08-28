using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackText : TrackPiece{
    protected override void Setup() {
        Name = "Text";
        Layer = 2;
        IsResizable = true;
    }
}