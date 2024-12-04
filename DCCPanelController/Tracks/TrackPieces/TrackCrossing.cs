using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

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