using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackLabel : TrackPiece{
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "Label", 0);
    }
}