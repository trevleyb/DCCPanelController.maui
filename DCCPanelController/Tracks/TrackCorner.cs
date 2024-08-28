using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackCorner : TrackPiece  {
    protected override void Setup() {
        Name = "Corner";
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   "Normal", "CornerR", 0);
        AddTrackImage(45,  "Normal", "CornerL", 270);
        AddTrackImage(90,  "Normal", "CornerR", 90);
        AddTrackImage(135, "Normal", "CornerL", 0);
        AddTrackImage(180, "Normal", "CornerR", 180);
        AddTrackImage(225, "Normal", "CornerL", 90);
        AddTrackImage(270, "Normal", "CornerR", 270);
        AddTrackImage(315, "Normal", "CornerL", 180);
    } 
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }

}