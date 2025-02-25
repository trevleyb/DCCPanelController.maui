using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackBase : ObservableObject {
    
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty] private int _x;          // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;          // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _z = 1;      // What position (layer) should this exist at 
    [ObservableProperty] private int _width = 1;  // What width is this component?
    [ObservableProperty] private int _height = 1; // What Height is this component? 
    [ObservableProperty] private int _layer = 1;  // What layer is this on? Higher sits on top of lower
    
    [ObservableProperty] private int _imageRotation;
    [ObservableProperty] private int _trackRotation; 

    [ObservableProperty] [property:JsonIgnore] 
    [NotifyPropertyChangedFor(nameof(ShowBelowSymbol))] 
    private bool _showAboveSymbol;
    
    [JsonIgnore] 
    public bool ShowBelowSymbol => !ShowAboveSymbol;

    [ObservableProperty] 
    [property:JsonIgnore] private bool _isSelected;  // Used in Design Mode. Is this track selected? 
    
    [JsonIgnore] protected SvgImage? ActiveImage = null;
    [JsonIgnore] protected int RotationIncrement = 45;
    [JsonIgnore] protected readonly double Scale = 1.5;
    
    [JsonIgnore] protected readonly StyleTrackImages StyleTrackImages = new();
    [JsonIgnore] public TrackConnectionsEnum[] Connections => ActiveImage?.Connections.ConnectionPointsRotated(ImageRotation) ?? SvgCompass.NoConnections();
    [JsonIgnore] public Panel? Parent { get; set; }

    [JsonIgnore] public IView? TrackViewRef { get; set; }
    [JsonIgnore] public ImageSource SymbolView => GetViewForSymbol(48) ?? throw new ApplicationException("Unable to set the symbol");

    protected TrackBase(Panel? parent = null) {
        Initialise();
        OnPropertyChanged(nameof(TrackView));
        if (parent != null) Parent = parent;
    }
    
    protected void Initialise() {
        Setup();
    }

    // This is needed for the JSON 
    public virtual string TrackType {
        get => GetType().Name;
        set => _ = value;
    }

    protected abstract void Setup();
    public virtual void CleanUp() { }
    
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

    //  This is a helper function as most track pieces need to create a View from an Image
    // ---------------------------------------------------------------------------------------------------------
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

    protected T Clone<T>(Panel parent) where T : ITrack {
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