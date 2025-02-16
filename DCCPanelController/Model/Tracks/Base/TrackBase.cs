using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackBase : ObservableObject {
    [ObservableProperty] private int _height = 1; // What height is this component?
    [ObservableProperty] private int _imageRotation;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private int _layer = 1; // What layer is this on? Only 1 element per layer.

    [ObservableProperty] private string _name = "Track Piece"; // Name of this particular track piece or object

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowBelowSymbol))] [property: JsonIgnore]
    private bool _showAboveSymbol;

    [ObservableProperty] private int _trackRotation; // What is the expected direction of the Track Piece?
    [ObservableProperty] private int _width = 1;     // What width is this component?
    [ObservableProperty] private int _x;             // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;             // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _z = 1;         // What position (layer) should this exist at 

    [JsonIgnore] protected SvgImage? ActiveImage = null;
    [JsonIgnore] protected int RotationIncrement = 45;

    [JsonIgnore] protected double Scale = 1.5;

    protected StyleTrackImages StyleTrackImages = new();

    protected TrackBase(Panel? parent = null) {
        Initialise();
        OnPropertyChanged(nameof(TrackView));
        PropertyChanged += OnPropertyChanged;
        if (parent != null) Parent = parent;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonIgnore] public TrackConnectionsEnum[] Connections => ActiveImage?.Connections.ConnectionPointsRotated(ImageRotation) ?? SvgCompass.NoConnections();
    [JsonIgnore] public Panel? Parent { get; set; }
    [JsonIgnore] public bool ShowBelowSymbol => !ShowAboveSymbol;

    [JsonIgnore] public IView? TrackViewRef { get; set; }
    [JsonIgnore] public ImageSource SymbolView => GetViewForSymbol(48) ?? throw new ApplicationException("Unable to set the symbol");

    public virtual string TrackObjectType {
        get => GetType().Name;
        set => _ = value;
    }

    public IView TrackView(double gridSize, bool passthrough = false) {
        TrackViewRef = GetViewForTrack(gridSize, passthrough);
        return TrackViewRef;
    }

    // This routine can be overriden by other routines up the chain. It is to work out how the track
    // piece should be displayed and then to create a common IView that can be shown on the screen. Most
    // track pieces are images, but some may not be such as a Text Block.
    // ---------------------------------------------------------------------------------------------------------
    protected abstract IView GetViewForTrack(double gridSize, bool passthrough = false);
    protected abstract ImageSource GetViewForSymbol(double gridSize);

    /// <summary>
    ///     This is a helper function as most track pieces need to create a View from an Image
    /// </summary>
    /// <param name="image">The image source to use</param>
    /// <param name="imageRotation">How the image should be rotated</param>
    /// <param name="gridSize">The grid size as this sets the Width and Height of the image</param>
    /// <param name="passthrough">Should this item be a pass-through for taps</param>
    /// <returns></returns>
    protected IView CreateViewFromImage(ImageSource image, int imageRotation, double gridSize, bool passthrough = false) {
        return new Image {
            Source = image,
            Scale = Scale,
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

    public void RotateLeft() {
        TrackRotation -= RotationIncrement;
        if (TrackRotation < 0) TrackRotation += 360;
        OnPropertyChanged(nameof(TrackView));
    }

    public void RotateRight() {
        TrackRotation += RotationIncrement;
        if (TrackRotation > 360) TrackRotation -= 360;
        OnPropertyChanged(nameof(TrackView));
    }

    protected void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected T Clone<T>(Panel parent) where T : ITrackPiece {
        var original = JsonSerializer.Serialize(this, SettingsService.PanelSerializerOptions);
        var copy = JsonSerializer.Deserialize<T>(original, SettingsService.PanelSerializerOptions);
        if (copy is { } clone) {
            clone.Id = Guid.NewGuid();
            clone.Parent = parent;
            return clone;
        }

        throw new Exception("Failed to clone panel");
    }
}