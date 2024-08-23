using DCCPanelController.Components.TrackPieces.Base;

namespace DCCPanelController.Components.TrackPieces;

public class TrackTerminator :TrackPiece  {
    protected override void Setup() {
        AddTrackImage(0,   "Normal", "Terminator1", -90);
        AddTrackImage(45,  "Normal", "Terminator2", 0);
        AddTrackImage(90,  "Normal", "Terminator1", 0);
        AddTrackImage(135, "Normal", "Terminator2", 90);
        AddTrackImage(180, "Normal", "Terminator1", 90);
        AddTrackImage(225, "Normal", "Terminator2", 0);
        AddTrackImage(270, "Normal", "Terminator1", 0);
        AddTrackImage(315, "Normal", "Terminator2", -90);
    }

}