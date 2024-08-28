using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackThreeway : TrackPiece, ITrackThreeway {
    protected override void Setup() {
        Name = "Threeway";
    }
    
    protected override void AddTrackImages() {
        AddTrackImage(0,   UnknownState, "Threeway1", 0);
        AddTrackImage(90,  UnknownState, "Threeway1", 90);
        AddTrackImage(180, UnknownState, "Threeway1", 180);
        AddTrackImage(270, UnknownState, "Threeway1", 270);

        AddTrackImage(0,   "Straight", "Threeway2", 0);
        AddTrackImage(90,  "Straight", "Threeway2", 90);
        AddTrackImage(180, "Straight", "Threeway2", 180);
        AddTrackImage(270, "Straight", "Threeway2", 270);

        AddTrackImage(0,   "Diverging-Left", "Threeway3", 0);
        AddTrackImage(90,  "Diverging-Left", "Threeway3", 90);
        AddTrackImage(180, "Diverging-Left", "Threeway3", 180);
        AddTrackImage(270, "Diverging-Left", "Threeway3", 270);

        AddTrackImage(0,   "Diverging-Right", "Threeway4", 0);
        AddTrackImage(90,  "Diverging-Right", "Threeway4", 90);
        AddTrackImage(180, "Diverging-Right", "Threeway4", 180);
        AddTrackImage(270, "Diverging-Right", "Threeway4", 270);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle(UnknownState, "Mainline");
        AddTrackStyle("Straight", "Mainline-Straight");
        AddTrackStyle("Diverging-Left", "Mainline-Diverging");
        AddTrackStyle("Diverging-Right", "Mainline-Diverging");
    }
}