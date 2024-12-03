using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackCrossing : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "90 degree crossing")]
    private string _name = "Crossing";

    protected override void Setup() {
        SetTrackSymbol("Cross1");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross1", (0, -90), (90 ,0), (180 ,90), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Cross2", (45, 0), (135 ,90), (225 ,0), (315, -90));
    }
}