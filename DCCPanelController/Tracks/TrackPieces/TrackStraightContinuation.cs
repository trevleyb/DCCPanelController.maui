using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackStraightContinuation : TrackContinuationBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Stright that continues on another page")]
    private string _name = "Stright (Continuation)";

    protected override void Setup() {
        SetTrackSymbol("ContinuationSA1");
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA2", (45, 0), (135 ,90), (225 ,180), (315, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL2", (45, 0), (135 ,90), (225 ,180), (315, 270));
    }
}