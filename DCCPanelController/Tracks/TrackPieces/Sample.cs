using DCCPanelController.Tracks.Base;

namespace DCCPanelController.Tracks;

public class Sample : TrackPiece {
    protected override void Setup() {
        Name = "Sample";
    }

    protected override void AddTrackImages() {
        AddTrackImage(0,   "Normal", "Sample", 0);
        AddTrackImage(0,   "Normal", "Sample", 90);
        AddTrackImage(0,   "Normal", "Sample", 180);
        AddTrackImage(0,   "Normal", "Sample", 270);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal", "Mainline");
    }
}