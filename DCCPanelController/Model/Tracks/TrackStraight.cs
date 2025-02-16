using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackStraight(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPieceBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackStraight() : this(null) { }

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackStraight>(parent);
    }

    protected override void Setup() {
        Name = "Straight";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Straight1", (0, 0), (90, 90), (180, 0), (270, 90));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Straight1", (0, 0), (90, 90), (180, 0), (270, 90));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Straight2", (45, 0), (135, 90), (225, 0), (315, 90));
    }
}