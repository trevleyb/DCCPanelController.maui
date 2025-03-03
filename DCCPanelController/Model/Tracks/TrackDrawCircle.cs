using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackDrawCircle(Panel? parent = null) : TrackDraw(parent), ITrackSymbol, ITrack {
    [ObservableProperty] [property: EditableColor(Name = "Circle Color", Description = "Border Color", Group = "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableColor(Name = "Border Color", Description = "Border Color", Group = "Border")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt(Name = "Border Width", Description = "Border With", Group = "Border")]
    private int _borderWidth = 1;

    public TrackDrawCircle() : this(null) { }

    [property: EditableInt(Name = "Width", Description = "Width of the Circle", Group = "Attributes")]
    public new int Width {
        get => base.Width;
        set => base.Width = value;
    }

    [property: EditableInt(Name = "Height", Description = "Height of the Circle", Group = "Attributes")]
    public new int Height {
        get => base.Height;
        set => base.Height = value;
    }

    [property: EditableInt(Name = "Layer", Group = "Attributes", Description = "What Layer does this peice sit on?", MinValue = 1, MaxValue = 5, Order = 5)]
    public new int Layer {
        get => base.Layer;
        set => base.Layer = value;
    }
    
    public ITrack Clone(Panel parent) {
        return Clone<TrackDrawCircle>(parent);
    }

    public string Name => "Circle";

    protected override void Setup() {
        Layer = 1;
        RotationIncrement = 0;
        Width = 1;
        Height = 1;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Circle", (0, 0), (0, 0), (0, 0), (0, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Circle", (0, 0), (0, 0), (0, 0), (0, 0));
    }

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;

        // Apply the various styles that need to be applied based on the 
        // details that we have within the context of this track type
        // --------------------------------------------------------------------------------------------------
        ActiveImage = imageInfo;
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    protected override IView GetViewForTrack(double gridSize, bool? passthrough) {
        var circle = new Ellipse {
            Fill = BackgroundColor ?? Colors.Transparent,
            Stroke = BorderColor ?? Colors.Transparent,
            StrokeThickness = BorderWidth,
            WidthRequest = gridSize * Width,
            HeightRequest = gridSize * Height,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            ZIndex = Layer,
            Opacity = Opacity,
            InputTransparent = passthrough ?? Passthrough
        };

        return circle;
    }
}