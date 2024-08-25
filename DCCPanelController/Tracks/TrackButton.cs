using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackButton : TrackPiece, ITrackButton  {
    protected override void Setup() {
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
    }
}