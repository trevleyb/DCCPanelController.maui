using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLabelCircle(Panel? parent = null) : Track(parent), ITrackSymbol, ITrack {

    // TODO: Thoughts, could a Label Circle be a special case that is linked to 
    //       a turnout and shows the ID of the turnout?? And a Line to the Turnout?
    public string Name => "Circle Image";

    [ObservableProperty] [property: EditableColor(Name = "Background", Description = "Background Color", Group = "Colors")]
    private Color _backgroundColor = Colors.Green;

    [ObservableProperty] [property: EditableColor(Name = "Border", Description = "Border Color", Group = "Colors")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt(Name = "Font Size", Description = "Font Size", Group = "Attributes")]
    private int _fontSize = 8;

    [ObservableProperty] [property: EditableEnum(Name = "Font Weight", Description = "Font Weight", Group = "Attributes")]
    private FontWeight _fontWeight = FontWeight.Regular;

    [ObservableProperty]
    [property: EditableString(Name = "Circle Label", Description = "Label to display in the Circle")]
    private string _label = string.Empty;

    [ObservableProperty] [property: EditableColor(Name = "Font Color", Description = "Font Color", Group = "Colors")]
    private Color _textColor = Colors.White;

    public TrackLabelCircle() : this(null) { }

    public ITrack Clone(Panel parent) {
        var clone = Clone<TrackLabelCircle>(parent);
        clone.Label = "";
        return clone;
    }

    protected override void Setup() {
        Layer = 2;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
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
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Button, TrackStyleImageEnum.Normal, Parent?.Defaults);
        style = SvgStyles.AddTextToStyle(style, Label);
        style = new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).WithTextSize(FontSize)).Build();
        ;
        if (!TextColor.Equals(Colors.Transparent)) style = SvgStyles.SetTextToColor(style, TextColor);
        if (!BackgroundColor.Equals(Colors.Transparent)) style = SvgStyles.SetButtonColor(style, BackgroundColor);
        if (!BorderColor.Equals(Colors.Transparent)) style = SvgStyles.SetButtonOutlineColor(style, BorderColor);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
}