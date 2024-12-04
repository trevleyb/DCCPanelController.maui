using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackStraight : TrackPieceBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "Straight Track")]
    private string _name = "Straight";

    protected override void Setup() {
        SetTrackSymbol("Straight1");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Straight1", (0, -90), (90, 0), (180, 90), (270, 0));
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Straight2", (45, 0), (135, 90), (225, 0), (315, -90));
    }
}