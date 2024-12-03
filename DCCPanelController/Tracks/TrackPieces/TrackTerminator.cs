using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

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