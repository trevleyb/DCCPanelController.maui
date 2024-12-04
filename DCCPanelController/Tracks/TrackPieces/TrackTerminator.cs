using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackTerminator : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Track Terminator")]
    private string _name = "Terminator";

    protected override void Setup() {
        SetTrackSymbol("Terminator1");
        AddImageSourceAndRotation(TrackStyleImage.Normal,  "Terminator1", (0, 0), (90 ,90), (180 ,180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal,  "Terminator2", (45, 90), (135 ,180), (225 ,270), (315, 0));
    }
}