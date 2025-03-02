using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackCornerContinuation(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackContinuation(parent, styleTypeEnum), ITrackSymbol, ITrack {
    public TrackCornerContinuation() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackCornerContinuation>(parent);
    }

    public string Name => "Corner Track";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationCL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationCL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}