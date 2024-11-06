using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackStraightContinuation : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Straight Continued...";
        DefaultState = "Normal";
        SetTrackSymbol("ContinuationSA1");
    }

    protected override void AddTrackImages() {
        AddTrackImage(0, "Arrow", "ContinuationSA1", 0);
        AddTrackImage(45, "Arrow", "ContinuationSA2", 90);
        AddTrackImage(90, "Arrow", "ContinuationSA1", 90);
        AddTrackImage(135, "Arrow", "ContinuationSA2", 180);
        AddTrackImage(180, "Arrow", "ContinuationSA1", 180);
        AddTrackImage(225, "Arrow", "ContinuationSA2", 270);
        AddTrackImage(270, "Arrow", "ContinuationSA1", 270);
        AddTrackImage(315, "Arrow", "ContinuationSA2", 0);

        AddTrackImage(0, "Lines", "ContinuationSL1", 0);
        AddTrackImage(45, "Lines", "ContinuationSL2", 90);
        AddTrackImage(90, "Lines", "ContinuationSL1", 90);
        AddTrackImage(135, "Lines", "ContinuationSL2", 180);
        AddTrackImage(180, "Lines", "ContinuationSL1", 180);
        AddTrackImage(225, "Lines", "ContinuationSL2", 270);
        AddTrackImage(270, "Lines", "ContinuationSL1", 270);
        AddTrackImage(315, "Lines", "ContinuationSL2", 0);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Arrow", "Mainline");
        AddTrackStyle("Lines", "Mainline");
    }
}