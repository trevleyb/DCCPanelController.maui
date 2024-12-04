using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class Sample : TrackPieceBase, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "A Sample Track Piece")]
    private string _name = "Sample";

    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Sample");
    }
}