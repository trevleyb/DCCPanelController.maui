using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackImage(Panel? parent = null) : TrackPieceBase(parent), ITrackSymbol, ITrackPiece {
    public TrackImage() : this(null) { }
    
    [EditableImageProperty(Name = "Image", Description = "Image to display")]
    public string TrackImageSource { get; set; } = "";
    
    [ObservableProperty] [property: EditableIntProperty(Name = "Width", Description = "Text Grid Width", Group = "Attributes")]
    private int _imageWidth = 2;

    [ObservableProperty] [property: EditableIntProperty(Name = "Height", Description = "Text Grid Height", Group = "Attributes")]
    private int _imageHeight = 2;
    
    public ImageSource? Image => ImageHelper.ImageFromBase64(TrackImageSource);
    
    protected override void Setup() {
        Layer = 2;
        Name = "ImageStyle";
        RotationIncrement = 90;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Image", (0, 0), (90, 0), (180, 0), (270, 0));
    }
    
    private double ImageWidthRequest(double gridSize) => (TrackRotation % 360 == 0 || TrackRotation % 360 == 180) ? gridSize * MaxGridWidth : gridSize; 
    private double ImageHeightRequest(double gridSize) => (TrackRotation % 360 == 90 || TrackRotation % 360 == 270) ? gridSize * MaxGridHeight : gridSize; 
    
    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        if (string.IsNullOrEmpty(TrackImageSource)) {
            var defaultImage = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough);
            return CreateViewFromImage(defaultImage.Image, defaultImage.Rotation, gridSize, passthrough); 
        }
        
        var image = new Image {
            Source = ImageHelper.ImageFromBase64(TrackImageSource),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            ZIndex = Layer,
            RotationX = TrackRotation,
            InputTransparent = passthrough,
            WidthRequest = gridSize * MaxGridWidth,
            HeightRequest = gridSize * MaxGridHeight,
        };
        return (IView)image;
    }

    private int MaxGridWidth => (Parent is not null) ? Width <= Parent.Cols - X ? Width : Parent.Cols - X : Width;
    private int MaxGridHeight => (Parent is not null) ? Height <= Parent.Rows - Y ? Height : Parent.Rows - Y : Height;

    public ITrackPiece Clone(Panel parent) {
        return Clone<TrackImage>(parent);
    }
    
    
    
    
}