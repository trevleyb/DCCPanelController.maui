using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackLabel : TrackPieceBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "Text Label")]
    private string _name = "Label";

    protected override void Setup() {
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label", (0, 0), (90, 0), (180, 0), (270, 0));
    }
}