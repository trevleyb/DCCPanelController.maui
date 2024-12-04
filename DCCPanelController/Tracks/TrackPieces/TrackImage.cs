using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

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