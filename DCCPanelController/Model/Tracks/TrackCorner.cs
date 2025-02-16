using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackCorner(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackCorner() : this(null) { }

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackCorner>(parent);
    }

    protected override void Setup() {
        Name = "Corner";
        ShowAboveSymbol = true;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerR", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerL", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerR", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerL", (45, 270), (135, 0), (225, 90), (315, 180));
    }
}