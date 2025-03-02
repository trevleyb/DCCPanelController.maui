using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackCorner(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackPiece(parent, styleTypeEnum), ITrackSymbol, ITrack {
    public TrackCorner() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackCorner>(parent);
    }

    public string Name => "Corner Track";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerR", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "CornerL", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerR", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "CornerL", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}