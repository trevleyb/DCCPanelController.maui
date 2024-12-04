using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackText : TrackBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStrProperty(Name = "Name (ID)", Description = "text Block")]
    private string _name = "Text";

    protected override SvgImage ActiveImage => SvgImages.Default();
    protected override SvgImage SymbolImage => SvgImages.Default();

    protected override void Setup() {
        Layer = 2;
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label");
    }
}