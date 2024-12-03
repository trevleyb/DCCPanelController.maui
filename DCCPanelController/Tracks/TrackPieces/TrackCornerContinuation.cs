using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackCornerContinuation : TrackPieceBase, ITrackSymbol, ITrackPiece {
    
    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Corner that continues on another page")]
    private string _name = "Corner (Continuation)";
    
    [ObservableProperty] 
    [property: EditableTrackImageTypePropertyAttribute(Name = "Name (ID)", Description = "Right Hand Turnout", TrackTypes = new [] { TrackStyleImage.Arrow , TrackStyleImage.Lines})]
    private TrackStyleImage _subType = TrackStyleImage.Arrow;
    
    protected override void Setup() {
        SetTrackSymbol("ContinuationCA1");
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Arrow, "ContinuationCA2", (45, 90), (135 ,180), (225 ,270), (315, 0));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Lines, "ContinuationCL2", (45, 90), (135 ,180), (225 ,270), (315, 0));
    }
}