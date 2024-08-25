using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackStraight : TrackPiece {
    
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "Straight1", -90);
        AddTrackImage(45,  "Normal", "Straight2", 0);
        AddTrackImage(90,  "Normal", "Straight1", 0);
        AddTrackImage(135, "Normal", "Straight2", 90);
        AddTrackImage(180, "Normal", "Straight1", 90);
        AddTrackImage(225, "Normal", "Straight2", 0);
        AddTrackImage(270, "Normal", "Straight1", 0);
        AddTrackImage(315, "Normal", "Straight2", -90);
    }
    
}
