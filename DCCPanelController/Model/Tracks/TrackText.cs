using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackText(Panel? parent = null) : TrackBase(parent), ITrackSymbol, ITrackPiece {
    
    public TrackText() : this(null) { }
    
    [ObservableProperty] [property: EditableStringProperty(Name = "Text", Description = "Text to Display")]
    private string _text = "Text";

    protected override void Setup() {
        Layer = 2;
        Name = "Text Block";
        SetTrackSymbol("Text");
        AddImageSourceAndRotation(TrackStyleImage.Normal, "Text");
    }
    
    public override ITrackPiece Clone() {
        return ObjectCloner.Clone(this) ?? throw new ArgumentException($"Cannot clone the Track '{this.GetType().Name}'");
    }

    [JsonIgnore]
    protected override SvgImage ActiveImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Normal, TrackRotation);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            ImageRotation = trackInfo.ImageRotation;
            TrackRotation = trackInfo.TrackRotation;
            var style = SvgStyles.GetStyle(TrackStyleType.Text, TrackStyleImage.Normal, Parent?.Defaults);
            return imageInfo.ApplyStyle(style);
        }
    }

    [JsonIgnore]
    protected override SvgImage SymbolImage {
        get {
            // Find the appropriate image reference for the details we have
            // ---------------------------------------------------------------------------------------------------
            var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(TrackStyleImage.Symbol, 0);
            var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
            var style = SvgStyles.GetStyle(TrackStyleType.Text, TrackStyleImage.Normal, Parent?.Defaults);
            return imageInfo.ApplyStyle(style);
        }
    }
}