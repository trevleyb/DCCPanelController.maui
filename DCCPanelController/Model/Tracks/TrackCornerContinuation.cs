using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCornerContinuation(Panel? parent = null) : TrackContinuationBase(parent), ITrackSymbol, ITrackPiece {
    public TrackCornerContinuation() : this(null) { }
    protected override void Setup() {
        SetTrackSymbol("ContinuationCA1");
        Name = "Corner Track (Continuation)";
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }

}