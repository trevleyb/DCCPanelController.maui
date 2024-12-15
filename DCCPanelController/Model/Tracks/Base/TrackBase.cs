using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.ViewModel;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackBase : ObservableObject {

    protected TrackBase(Panel? parent = null) {
        Initialise();
        OnPropertyChanged(nameof(TrackView));
        PropertyChanged += OnPropertyChanged;
        if (parent != null) Parent = parent;
    }

    protected readonly StyleTrackImages StyleTrackImages = new();

    [DoNotClone] public Guid Id { get; set; } = Guid.NewGuid();
    [ObservableProperty] private string _name = "Track Piece"; // Name of this particular track piece or object
    [ObservableProperty] private int _layer  = 1;              // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private int _x;                       // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                       // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _trackRotation;           // What is the expected direction of the Track Piece?
    [ObservableProperty] private int _imageRotation;
    [ObservableProperty] private bool _isSelected;

    [JsonIgnore] protected SvgImage? ActiveImage = null;
    [JsonIgnore] public TrackConnectionsEnum[] Connections => ActiveImage?.Connections.ConnectionPointsRotated(ImageRotation) ?? SvgCompass.NoConnections();
    [DoNotClone][JsonIgnore] public Panel? Parent { get; set; }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowBelowSymbol))] [property: JsonIgnore] private bool _showAboveSymbol = false;
    public bool ShowBelowSymbol => !ShowAboveSymbol;

    public IView TrackView (double gridSize, bool passthrough = false) => TrackViewRef ??= GetViewForTrack(gridSize, passthrough);
    [DoNotClone][JsonIgnore] public IView? TrackViewRef { get; set; }
    [DoNotClone][JsonIgnore] public ImageSource SymbolView => GetViewForSymbol(48) ?? throw new ApplicationException("Unable to set the symbol");

    // This routine can be overriden by other routines up the chain. It is to work out how the track
    // piece should be displayed and then to create a common IView that can be shown on the screen. Most
    // track pieces are images, but some may not be such as a Text Block.
    // ---------------------------------------------------------------------------------------------------------
    protected abstract IView GetViewForTrack(double gridSize, bool passthrough = false);
    protected abstract ImageSource GetViewForSymbol(double gridSize);

    /// <summary>
    /// This is a helper function as most track pieces need to create a View from an Image
    /// </summary>
    /// <param name="image">The image source to use</param>
    /// <param name="imageRotation">How the image should be rotated</param>
    /// <param name="gridSize">The grid size as this sets the Width and Height of the image</param>
    /// <param name="passthrough">Should this item be a pass-through for taps</param>
    /// <returns></returns>
    protected IView CreateViewFromImage(ImageSource image, int imageRotation, double gridSize, bool passthrough = false) {
        return new Image {
            Source = image,
            Scale = 1.5,
            ZIndex = Layer,
            Rotation = imageRotation,
            WidthRequest = gridSize,
            HeightRequest = gridSize,
            InputTransparent = passthrough,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
    }

    /// <summary>
    ///     Manage when properties have changed as we may need to redraw the image
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    protected void Initialise() {
        Setup();
    }

    protected abstract void Setup();
    
    public virtual string TrackObjectType {
        get => GetType().Name;
        set => _ = value;
    }
    
    public void RotateLeft() {
        TrackRotation -= 45;
        if (TrackRotation < 0) TrackRotation += 360;
        OnPropertyChanged(nameof(TrackView));
    }

    public void RotateRight() {
        TrackRotation += 45;
        if (TrackRotation > 360) TrackRotation -= 360;
        OnPropertyChanged(nameof(TrackView));
    }
    
    protected void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }
}