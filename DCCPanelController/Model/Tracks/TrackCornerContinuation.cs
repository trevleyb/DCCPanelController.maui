using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackCornerContinuation(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackContinuationBase(parent, styleTypeEnum), ITrackSymbol, ITrackPiece {
    public TrackCornerContinuation() : this(null) { }

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackCornerContinuation>(parent);
    }

    protected override void Setup() {
        Name = "Corner(C)";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationCL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationCL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}