using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackLabel : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Label";
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   "Normal", "Label", 0);
        Layer = 2;
    }
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }

}