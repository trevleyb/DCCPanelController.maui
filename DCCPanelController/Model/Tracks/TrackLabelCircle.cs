using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLabelCircle(Panel? parent = null) : TrackLabelBase(parent), ITrackSymbol, ITrackPiece {
    public TrackLabelCircle() : this(null) { }
    protected override void Setup() {
        Layer = 2;
        Name="Label";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "Label", (0, 0),(45,45), (90, 90), (135,135), (180, 180),(225,225), (270, 270), (315,315));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label", (0, 0),(45,45), (90, 90), (135,135), (180, 180),(225,225), (270, 270), (315,315));
    }
}