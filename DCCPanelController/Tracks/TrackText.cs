using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackText : TrackPiece{
    protected override void Setup() {
        Layer = 2;
        IsResizable = true;
    }
}