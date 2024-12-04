using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackCornerContinuation : TrackContinuationBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "Corner that continues on another page")]
    private string _name = "Corner (Continuation)";

    protected override void Setup() {
        SetTrackSymbol("ContinuationCA1");
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA2", (45, 0), (135, 90), (225, 180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL1", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL2", (45, 0), (135, 90), (225, 180), (315, 270));
    }
}