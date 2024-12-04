using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackCorner : TrackPieceBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "Corner Piece")]
    private string _name = "Corner";

    protected override void Setup() {
        SetTrackSymbol("CornerR");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "CornerR", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "CornerL", (45, 270), (135, 0), (225, 90), (315, 180));
    }
}