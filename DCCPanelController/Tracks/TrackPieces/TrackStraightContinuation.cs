using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackStraightContinuation : TrackContinuationBase, ITrackSymbol, ITrackPiece {
    protected override void Setup() {
        SetTrackSymbol("ContinuationSA1");
        Name = "Straight Track (Continuation)";
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}