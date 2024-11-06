using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackCrossing : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Crossing";
        DefaultState = "Normal";
        SetTrackSymbol("Cross1");
    }

    protected override void AddTrackImages() {
        AddTrackImage(0, "Normal", "Cross1", -90);
        AddTrackImage(45, "Normal", "Cross2", 0);
        AddTrackImage(90, "Normal", "Cross1", 0);
        AddTrackImage(135, "Normal", "Cross2", 90);
        AddTrackImage(180, "Normal", "Cross1", 90);
        AddTrackImage(225, "Normal", "Cross2", 0);
        AddTrackImage(270, "Normal", "Cross1", 0);
        AddTrackImage(315, "Normal", "Cross2", -90);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal", "Mainline");
    }
}