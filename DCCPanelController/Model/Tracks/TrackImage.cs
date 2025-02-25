using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : TrackSpecialBase(parent), ITrackSymbol, ITrack {

    [ObservableProperty]
    private string _name = "Image";
    
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

    [ObservableProperty] [property: EditableBool(Name = "Aspect Ratio", Description = "Keep Aspect Ratio", Group = "Attributes")]
    private bool _keepAspectRatio = true;

    public TrackImage() : this(null) { }

    [EditableImage(Name = "Image", Group="Image", Description = "Image to display")]
    public string TrackImageSource { get; set; } = "";

    public ImageSource? Image => ImageHelper.ImageFromBase64(TrackImageSource);

    private int MaxGridWidth => Parent is not null ? Width <= Parent.Cols - X ? Width : Parent.Cols - X : Width;
    private int MaxGridHeight => Parent is not null ? Height <= Parent.Rows - Y ? Height : Parent.Rows - Y : Height;

    public ITrack Clone(Panel parent) {
        return Clone<TrackImage>(parent);
    }

    protected override void Setup() {
        Layer = 0;
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

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        if (string.IsNullOrEmpty(TrackImageSource)) {
            var defaultImage = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough);
            return CreateViewFromImage(defaultImage.Image, defaultImage.Rotation, gridSize, passthrough);
        }

        var image = new Image {
            Source = ImageHelper.ImageFromBase64(TrackImageSource),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            ZIndex = Layer,
            Aspect = KeepAspectRatio ? Aspect.AspectFit : Aspect.Fill,
            RotationX = TrackRotation,
            InputTransparent = passthrough,
            WidthRequest = gridSize * MaxGridWidth,
            HeightRequest = gridSize * MaxGridHeight
        };

        return image;
    }
}