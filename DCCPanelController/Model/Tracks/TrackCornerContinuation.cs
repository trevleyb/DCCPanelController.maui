using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackCornerContinuation(Panel? parent = null, TrackStyleType styleType = TrackStyleType.Mainline) : TrackContinuationBase(parent, styleType), ITrackSymbol, ITrackPiece {
    public TrackCornerContinuation() : this(null) { }
    
    protected override void Setup() {
        Name = "Corner(C)";
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Symbol, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
    public ITrackPiece Clone(Panel parent) {
        var track = (TrackCornerContinuation)MemberwiseClone();
        track.Id = Guid.NewGuid();
        track.Parent = parent;
        return track;
    }
}