using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackLabelCircle(Panel? parent = null) : Track(parent), ITrackSymbol, ITrack {
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

    [property: EditableInt(Name = "Layer", Group = "Attributes", Description = "What Layer does this peice sit on?", MinValue = 1, MaxValue = 5, Order = 5)]
    public new int Layer {
        get => base.Layer;
        set => base.Layer = value;
    }

   [property: EditableDouble(Name = "Size", Group = "Attributes", Description = "What Size should this label be?", MinValue = 1, MaxValue = 3, Order = 5)]
        public new double Scale {
            get => base.Scale;
            set => base.Scale = value;
        }

    public TrackLabelCircle() : this(null) { }

    public ITrack Clone(Panel parent) {
        var clone = Clone<TrackLabelCircle>(parent);
        return clone;
    }

    // TODO: Thoughts, could a Label Circle be a special case that is linked to 
    //       a turnout and shows the ID of the turnout?? And a Line to the Turnout?
    public string Name => "Circle Image";

    protected override void Setup() {
        Layer = 4;
        Scale = 2;
        Passthrough = true; // Don't accept clicks on this item
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Label", (0, 0), (45, 45), (90, 90), (135, 135), (180, 180), (225, 225), (270, 270), (315, 315));
    }

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize, Passthrough).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool? passthrough) {
        var image = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough ?? Passthrough);
        return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough ?? Passthrough);
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Button, TrackStyleImageEnum.Normal, Parent);
        style = SvgStyles.AddTextToStyle(style, Label);
        style = new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).WithTextSize(FontSize)).Build();
        
        if (!TextColor.Equals(Colors.Transparent)) style = SvgStyles.SetTextToColor(style, TextColor);
        if (!BackgroundColor.Equals(Colors.Transparent)) style = SvgStyles.SetButtonColor(style, BackgroundColor);
        if (!BorderColor.Equals(Colors.Transparent)) style = SvgStyles.SetButtonOutlineColor(style, BorderColor);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }
}