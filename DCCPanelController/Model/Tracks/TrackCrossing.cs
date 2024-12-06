using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCrossing : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        SetTrackSymbol("Cross1");
        Name = "Crossing";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross1", (0, -90), (90, 0), (180, 90), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross2", (45, 0), (135, 90), (225, 0), (315, -90));
    }
    
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}