using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackCorner : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "Corner Piece")]
    private string _name = "Corner";

    protected override void Setup() {
        SetTrackSymbol("CornerR");
        AddImageSourceAndRotation(TrackStyleImage.Default, "CornerR", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImage.Default, "CornerL", (45, 270), (135, 0), (225, 90), (315, 180));
    }
}
