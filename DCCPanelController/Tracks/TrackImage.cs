using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackImage : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Image";
        Layer = 0;
    }
    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }

}