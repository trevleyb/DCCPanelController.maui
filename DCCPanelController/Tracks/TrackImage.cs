using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class TrackImage : TrackPiece {
    protected override void Setup() {
        Name = "Image";
        Layer = 0;
    }
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }

}