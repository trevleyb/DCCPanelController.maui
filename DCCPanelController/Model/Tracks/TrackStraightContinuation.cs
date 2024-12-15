using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackStraightContinuation(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackContinuationBase(parent, styleType), ITrackSymbol, ITrackPiece {
    public TrackStraightContinuation() : this(null) { }
    protected override void Setup() {
        Name = "Straight(C)";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }


}