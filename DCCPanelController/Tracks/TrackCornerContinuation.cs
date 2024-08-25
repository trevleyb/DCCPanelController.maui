using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackCornerContinuation : TrackPiece {
    protected override void Setup() {
        AddTrackImage(0,   "Arrow", "ContinuationCA1", 0);
        AddTrackImage(45,  "Arrow", "ContinuationCA2", 90);
        AddTrackImage(90,  "Arrow", "ContinuationCA1", 90);
        AddTrackImage(135, "Arrow", "ContinuationCA2", 180);
        AddTrackImage(180, "Arrow", "ContinuationCA1", 180);
        AddTrackImage(225, "Arrow", "ContinuationCA2", 270);
        AddTrackImage(270, "Arrow", "ContinuationCA1", 270);
        AddTrackImage(315, "Arrow", "ContinuationCA2", 0);
        
        AddTrackImage(0,   "Lines", "ContinuationCL1", 0);
        AddTrackImage(45,  "Lines", "ContinuationCL2", 90);
        AddTrackImage(90,  "Lines", "ContinuationCL1", 90);
        AddTrackImage(135, "Lines", "ContinuationCL2", 180);
        AddTrackImage(180, "Lines", "ContinuationCL1", 180);
        AddTrackImage(225, "Lines", "ContinuationCL2", 270);
        AddTrackImage(270, "Lines", "ContinuationCL1", 270);
        AddTrackImage(315, "Lines", "ContinuationCL2", 0);

    }
}