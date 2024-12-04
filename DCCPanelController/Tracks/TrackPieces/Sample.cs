using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class Sample : TrackPieceBase, ITrackPiece {
    
    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "A Sample Track Piece")]
    private string _name = "Sample";
    
    protected override void Setup() {
        AddImageSourceAndRotation(TrackStyleImage.Normal,   "Sample");
    }
}