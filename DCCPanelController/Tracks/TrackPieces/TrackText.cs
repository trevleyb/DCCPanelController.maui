using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.Tracks;

public class TrackText : TrackPiece, ITrackSymbol {
    protected override void Setup() {
        Name = "Text";
        DefaultState = "Normal";
        Layer = 2;
        IsResizable = true;
        SetTrackSymbol("Label");
    }

    protected override void AddTrackImages() {
        AddTrackImage(0, "Normal", "Label", 0);
    }

    protected override void AddTrackStyles() {
        AddTrackStyle("Normal", "Mainline");
    }
}