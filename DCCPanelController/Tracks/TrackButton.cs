using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackButton : TrackPiece, ITrackButton  {
    protected override void Setup() {
        Name = "Button";
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);
        AddTrackImage(0,"Active", "Button", 0);

        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
        AddTrackImage(0,"InActive", "Button", 0);
        Layer = 2;
    }
}