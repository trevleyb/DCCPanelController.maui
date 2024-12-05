using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.Tracks.TrackPieces.Base;
using DCCPanelController.Tracks.TrackPieces.Interfaces;

namespace DCCPanelController.Tracks.TrackPieces;

public partial class TrackText : TrackBase, ITrackSymbol, ITrackPiece {
    [ObservableProperty] [property: EditableStringProperty(Name = "Text", Description = "Text to Display")]
    private string _text = "Text";

    protected override SvgImage ActiveImage => SvgImages.Default();
    protected override SvgImage SymbolImage => SvgImages.Default();

    protected override void Setup() {
        Layer = 2;
        Name = "Text Block";
        SetTrackSymbol("Label");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Label");
    }
    public override ITrackPiece Clone() {
        var clone = (ITrackPiece)MemberwiseClone();
        return clone;
    }

}