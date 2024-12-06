using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCorner : TrackPieceBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        SetTrackSymbol("CornerR");
        Name = "Corner Track";
        AddImageSourceAndRotation(TrackStyleImage.Normal, "CornerR", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "CornerL", (45, 270), (135, 0), (225, 90), (315, 180));
    }
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}