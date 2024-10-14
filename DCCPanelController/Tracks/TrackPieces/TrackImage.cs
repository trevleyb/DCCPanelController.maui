using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackImage : TrackPiece, ITrackSymbol {

    protected override void Setup() {
        Name = "Image";
        DefaultState = "Normal";
        Layer = 0;
        SetTrackSymbol("Image");
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0, "Normal", "Image", 0);
        Layer = 2;
    }
    
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal", "Mainline");
    }

}