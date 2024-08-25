using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackButton : TrackPiece {
    protected override void Setup() {
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
    }
}