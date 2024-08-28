using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackLabel : TrackPiece{
    protected override void Setup() {
        Name = "Label";
        AddTrackImage(0,   "Normal", "Label", 0);
        Layer = 2;
    }
}