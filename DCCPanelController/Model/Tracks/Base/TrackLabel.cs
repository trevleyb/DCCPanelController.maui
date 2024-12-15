using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackLabelBase : TrackBase {

    protected TrackLabelBase(Panel? parent = null) : base(parent) { }
    
    [ObservableProperty] 
    [property: EditableStringProperty(Name = "Circle Label", Description = "Label to display in the Circle")]
    private string _circlelabel = string.Empty;

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImage.Symbol, TrackRotation, gridSize).Image;

    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackStyleImage.Normal, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImage trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;
        var style = SvgStyles.GetStyle(TrackStyleType.Button, TrackStyleImage.Normal, Parent?.Defaults);
        style = SvgStyles.AddTextToStyle(style, Circlelabel);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

}