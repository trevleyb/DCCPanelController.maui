using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : Track(parent), ITrackSymbol, ITrack {
    [ObservableProperty] [property: EditableColor(Name = "Border Color", Description = "Border Color", Group = "Border")]
    private Color _borderColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt(Name = "Border Radius", Description = "Rounder Corners on the Border", Group = "Border")]
    private int _borderRadius;

    [ObservableProperty] [property: EditableInt(Name = "Border Width", Description = "Border With", Group = "Border")]
    private int _borderWidth;

    [ObservableProperty] [property: EditableBool(Name = "Aspect Ratio", Description = "Keep Aspect Ratio", Group = "Attributes")]
    private bool _keepAspectRatio = true;

    public TrackImage() : this(null) { }

    [EditableImage(Name = "Image", Group = "Image", Description = "Image to display")]
    public string TrackImageSource { get; set; } = "";

    public ImageSource? Image => ImageHelper.ImageFromBase64(TrackImageSource);

    private int MaxGridWidth => Parent is not null ? Width <= Parent.Cols - X ? Width : Parent.Cols - X : Width;
    private int MaxGridHeight => Parent is not null ? Height <= Parent.Rows - Y ? Height : Parent.Rows - Y : Height;

    [property: EditableInt(Name = "Width", Description = "Width of the Image", Group = "Attributes")]
    public new int Width {
        get => base.Width;
        set => base.Width = value;
    }

    [property: EditableInt(Name = "Height", Description = "Height of the Image", Group = "Attributes")]
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
        return Clone<TrackImage>(parent);
    }

    public string Name => "Image";

    protected override void Setup() {
        Layer = 1;
        RotationIncrement = 90;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
    }

    private double ImageWidthRequest(double gridSize) {
        return TrackRotation % 360 == 0 || TrackRotation % 360 == 180 ? gridSize * MaxGridWidth : gridSize;
    }

    private double ImageHeightRequest(double gridSize) {
        return TrackRotation % 360 == 90 || TrackRotation % 360 == 270 ? gridSize * MaxGridHeight : gridSize;
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
        if (string.IsNullOrEmpty(TrackImageSource)) {
            var defaultImage = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough ?? Passthrough);
            return CreateViewFromImage(defaultImage.Image, defaultImage.Rotation, gridSize, passthrough ?? Passthrough);
        }

        var image = new Image {
            Source = ImageHelper.ImageFromBase64(TrackImageSource),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            ZIndex = Layer,
            Aspect = KeepAspectRatio ? Aspect.AspectFit : Aspect.Fill,
            RotationX = TrackRotation,
            InputTransparent = passthrough ?? Passthrough,
            WidthRequest = gridSize * MaxGridWidth,
            HeightRequest = gridSize * MaxGridHeight
        };

        return BorderWidth <= 0
            ? image
            : new Border {
                Content = image,
                InputTransparent = passthrough ?? Passthrough,
                RotationX = TrackRotation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                WidthRequest = gridSize * MaxGridWidth,
                HeightRequest = gridSize * MaxGridHeight,
                StrokeThickness = BorderWidth,
                Stroke = BorderColor,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(BorderRadius) }
            };
    }
}