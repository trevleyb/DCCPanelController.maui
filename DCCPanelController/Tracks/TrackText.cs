using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackText : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Text";
        Layer = 2;
        IsResizable = true;
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal","Mainline");
    }

}