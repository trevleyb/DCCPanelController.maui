using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public class TrackStraightContinuation(Panel? parent = null, TrackStyleTypeEnum styleTypeEnum = TrackStyleTypeEnum.Mainline) : TrackContinuation(parent, styleTypeEnum), ITrackSymbol, ITrack {
    public TrackStraightContinuation() : this(null) { }

    public ITrack Clone(Panel parent) {
        return Clone<TrackStraightContinuation>(parent);
    }

    public string Name => "Straight Track";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Arrow, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationSL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Lines, "ContinuationSL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}