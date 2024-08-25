using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackLabel : TrackPiece{
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "Label", 0);
    }
}