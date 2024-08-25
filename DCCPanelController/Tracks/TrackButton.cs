using DCCPanelController.Components.TrackPieces.Base;
using DCCPanelController.Components.TrackPieces.Interfaces;

namespace DCCPanelController.Components.TrackPieces;

public class TrackButton : TrackPiece, ITrackButton  {
    protected override void Setup() {
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
        AddTrackImage(0,"Normal", "Button", 0);
    }
}