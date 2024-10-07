using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackTerminator :TrackPiece, ITrackSymbol  {
    protected override void Setup() {
        Name = "Terminator";
        SetTrackSymbol("Terminator1");
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   "Normal", "Terminator1", 0);
        AddTrackImage(45,  "Normal", "Terminator2", 90);
        AddTrackImage(90,  "Normal", "Terminator1", 90);
        AddTrackImage(135, "Normal", "Terminator2", 180);
        AddTrackImage(180, "Normal", "Terminator1", 180);
        AddTrackImage(225, "Normal", "Terminator2", 270);
        AddTrackImage(270, "Normal", "Terminator1", 270);
        AddTrackImage(315, "Normal", "Terminator2", 0);
    }
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }


}