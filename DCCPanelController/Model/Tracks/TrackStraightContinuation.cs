using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackStraightContinuation(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackContinuationBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackStraightContinuation() : this(null) { }
    protected override void Setup() {
        Name = "Straight(C)";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationSL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationSL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackStraightContinuation>(parent);
    }
}