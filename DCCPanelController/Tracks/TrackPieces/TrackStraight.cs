using System.Text;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackStraight : TrackPiece, ITrackSymbol {
    
    protected override void Setup() {
        Name = "Straight";
        DefaultState = "Normal";
        SetTrackSymbol("Straight1");
    }

    protected override void AddTrackImages() {
        AddTrackImage(0,   "Normal", "Straight1", -90);
        AddTrackImage(45,  "Normal", "Straight2", 0);
        AddTrackImage(90,  "Normal", "Straight1", 0);
        AddTrackImage(135, "Normal", "Straight2", 90);
        AddTrackImage(180, "Normal", "Straight1", 90);
        AddTrackImage(225, "Normal", "Straight2", 0);
        AddTrackImage(270, "Normal", "Straight1", 0);
        AddTrackImage(315, "Normal", "Straight2", -90);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }
    
}
