using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackStraightContinuation : TrackPiece {
    protected override void Setup() {
        AddTrackImage(0,   "Arrow", "ContinuationSA1", -90);
        AddTrackImage(45,  "Arrow", "ContinuationSA2", 0);
        AddTrackImage(90,  "Arrow", "ContinuationSA1", 0);
        AddTrackImage(135, "Arrow", "ContinuationSA2", 90);
        AddTrackImage(180, "Arrow", "ContinuationSA1", 90);
        AddTrackImage(225, "Arrow", "ContinuationSA2", 0);
        AddTrackImage(270, "Arrow", "ContinuationSA1", 0);
        AddTrackImage(315, "Arrow", "ContinuationSA2", -90);
        
        AddTrackImage(0,   "Lines", "ContinuationSL1", -90);
        AddTrackImage(45,  "Lines", "ContinuationSL2", 0);
        AddTrackImage(90,  "Lines", "ContinuationSL1", 0);
        AddTrackImage(135, "Lines", "ContinuationSL2", 90);
        AddTrackImage(180, "Lines", "ContinuationSL1", 90);
        AddTrackImage(225, "Lines", "ContinuationSL2", 0);
        AddTrackImage(270, "Lines", "ContinuationSL1", 0);
        AddTrackImage(315, "Lines", "ContinuationSL2", -90);

    }
}