using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackDrawRectangle(Panel? parent = null) : TrackDraw(parent), ITrackSymbol, ITrack {
    [ObservableProperty] [property: EditableColor(Name = "Rectangle Color", Description = "Border Color", Group = "Border")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableColor(Name = "Border Color", Description = "Border Color", Group = "Border")]
    private Color _borderColor = Colors.Black;

    [ObservableProperty] [property: EditableInt(Name = "Border Width", Description = "Border With", Group = "Border")]
    private int _borderWidth = 1;

    public TrackDrawRectangle() : this(null) { }

    [property: EditableInt(Name = "Width", Description = "Width of the Rectangle", Group = "Attributes")]
    public new int Width {
        get => base.Width;
        set => base.Width = value;
    }

    [property: EditableInt(Name = "Height", Description = "Height of the Rectangle", Group = "Attributes")]
    public new int Height {
        get => base.Height;
        set => base.Height = value;
    }

    public ITrack Clone(Panel parent) {
        return Clone<TrackDrawRectangle>(parent);
    }

    public string Name => "Rectangle";

    protected override void Setup() {
        Layer = 1;
        RotationIncrement = 0;
        Width = 1;
        Height = 1;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Rectangle", (0, 0), (0, 0), (0, 0), (0, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Rectangle", (0, 0), (0, 0), (0, 0), (0, 0));
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

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        var rectangle = new Rectangle {
            Fill = BackgroundColor ?? Colors.Transparent,
            Stroke = BorderColor ?? Colors.Transparent,
            StrokeThickness = BorderWidth,
            WidthRequest = gridSize * Width,
            HeightRequest = gridSize * Height,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            ZIndex = Layer,
            Opacity = Opacity,
            InputTransparent = passthrough
        };

        return rectangle;
    }
}