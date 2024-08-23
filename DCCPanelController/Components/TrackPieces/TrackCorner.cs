using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackCorner : TrackPiece  {
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "CornerL", -90);
        AddTrackImage(45,  "Normal", "CornerR", 0);
        AddTrackImage(90,  "Normal", "CornerL", 0);
        AddTrackImage(135, "Normal", "CornerR", 90);
        AddTrackImage(180, "Normal", "CornerL", 90);
        AddTrackImage(225, "Normal", "CornerR", 0);
        AddTrackImage(270, "Normal", "CornerL", 0);
        AddTrackImage(315, "Normal", "CornerR", -90);
    } 
    
}