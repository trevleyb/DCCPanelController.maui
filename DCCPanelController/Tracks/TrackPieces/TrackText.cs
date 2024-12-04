using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Tracks.Base;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks;

public partial class TrackText : TrackBase, ITrackSymbol, ITrackPiece {

    [ObservableProperty]
    [property: EditableStrProperty(Name = "Name (ID)", Description = "text Block")]
    private string _name = "Text";

    protected override void Setup() {
        Layer = 2;
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label");
    }

    protected override SvgImage ActiveImage => SvgImages.Default();
    protected override SvgImage SymbolImage => SvgImages.Default();
}