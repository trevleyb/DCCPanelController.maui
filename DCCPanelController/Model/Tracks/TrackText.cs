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
        Name = "Text";
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Text");
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Text");
    }
    
    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var image = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Text, TrackStyleImageEnum.Normal, Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
    public ITrackPiece Clone(Panel parent) {
        return new TrackText(parent);
    }
}