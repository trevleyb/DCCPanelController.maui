using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackStraightContinuation : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Stright that continues on another page")]
    private string _name = "Stright (Continuation)";

    [ObservableProperty] 
    [property: EditableTrackImageTypePropertyAttribute(Name = "Name (ID)", Description = "Right Hand Turnout", TrackTypes = new [] { TrackStyleImage.Arrow , TrackStyleImage.Lines})]
    private TrackStyleImage _subType = TrackStyleImage.Arrow;
    
    protected override void Setup() {
        SetTrackSymbol("ContinuationSA1");
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationSA2", (45, 90), (135 ,180), (225 ,270), (315, 0));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationSL2", (45, 90), (135 ,180), (225 ,270), (315, 0));
    }
}