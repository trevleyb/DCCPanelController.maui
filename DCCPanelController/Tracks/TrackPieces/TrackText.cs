using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackText : TrackPieceBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "text Block")]
    private string _name = "Text";

    protected override void Setup() {
        Layer = 2;
        IsResizable = true;
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label");
    }
}