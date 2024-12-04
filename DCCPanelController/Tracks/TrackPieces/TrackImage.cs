using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackImage : TrackPieceBase, ITrackSymbol, ITrackPiece {
    
    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "Image")]
    private string _name = "Image";

    protected override void Setup() {
        Layer = 0;
        SetTrackSymbol("Image");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Image", (0, 0), (90 ,0), (180 ,0), (270, 0));
    }
}