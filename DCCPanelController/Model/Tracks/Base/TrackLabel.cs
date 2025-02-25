using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackLabelBase : TrackBase {

    [ObservableProperty]
    [property: EditableString(Name = "Circle Label", Description = "Label to display in the Circle")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableInt(Name = "Font Size", Description = "Font Size", Group = "Attributes")]
    private int _fontSize = 12;

    [ObservableProperty] [property: EditableEnum(Name = "Horizontal", Description = "Horizontal Justification of the Text", Group = "Attributes")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;

    [ObservableProperty] [property: EditableEnum(Name = "Vertical", Description = "Vertical Justification of the Text", Group = "Attributes")]
    private TextAlignment _verticalJustification = TextAlignment.Center;
    
    [ObservableProperty] [property: EditableColor(Name = "Font Color", Description = "Font Color", Group = "Colors")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: EditableColor(Name = "Background", Description = "Background Color", Group = "Colors")]
    private Color _backgroundColor = Colors.Transparent;

    protected TrackLabelBase(Panel? parent = null) : base(parent) { }

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
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Button, TrackStyleImageEnum.Normal, Parent?.Defaults);
        style = SvgStyles.AddTextToStyle(style, Label);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
}