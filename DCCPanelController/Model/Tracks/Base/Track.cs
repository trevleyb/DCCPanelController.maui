using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Microsoft.Maui.Controls.Shapes;
using Plugin.Maui.Audio;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class Track : ObservableObject {

    public Guid UniqueID { get; set; } = Guid.NewGuid();
    public TrackConnectionsEnum Connection(int direction) => ActiveImage?.Connections.ConnectionPointsRotatedForDirection(direction,ImageRotation) ?? TrackConnectionsEnum.None;
    private IView? _trackViewRef;

    [JsonIgnore] public Panel? Parent { get; set; }
    [JsonIgnore] protected double Scale = 1.5;
    [JsonIgnore] protected bool Passthrough = false;
    [JsonIgnore] protected readonly StyleTrackImages StyleTrackImages = new();

    [ObservableProperty] [property: JsonIgnore] private bool _isSelected;                      // Used in Design Mode. Is this track selected? 
    [ObservableProperty] [property: JsonIgnore] private bool _isPath;     // Used in Design Mode. Is this track selected? 
    [ObservableProperty] [property: JsonIgnore] private bool _isOccupied; // Used in Design Mode. Is this track selected? 

    [ObservableProperty] private int _x;          // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;          // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _z = 1;      // What position (layer) should this exist at 
    [ObservableProperty] private int _layer = 2;  // What layer is this on? Higher sits on top of lower
    [ObservableProperty] private int _width = 1;  // What width is this component?
    [ObservableProperty] private int _height = 1; // What Height is this component? 
    [ObservableProperty] private int _imageRotation;
    [ObservableProperty] private int _trackRotation;

    [JsonIgnore] protected SvgImage? ActiveImage = null;
    [JsonIgnore] protected int RotationIncrement = 45;
    [JsonIgnore] public ImageSource SymbolView => GetViewForSymbol(48) ?? throw new ApplicationException("Unable to set the symbol");

    protected Track(Panel? parent = null) {
        Initialise();
        if (parent != null) Parent = parent;
    }

    // This is needed for the JSON 
    public virtual string TrackType {
        get => GetType().Name;
        set => _ = value;
    }

    protected void Initialise() {
        Setup();
    }

    protected abstract void Setup();

    public void InvalidateView() {
        Console.WriteLine($"Track InvalidateView: {GetType().Name}");
        _trackViewRef = null;   
    }
    
    public IView TrackView(double gridSize, bool? passthrough) {
        return _trackViewRef ??= GetViewForTrack(gridSize, passthrough ?? Passthrough);
    }
    protected abstract IView GetViewForTrack(double gridSize, bool? passthrough);
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

    private IAudioPlayer? _clickSoundPlayer;
    protected void ClickSound(ClickSoundTypeEnum clickSoundType = ClickSoundTypeEnum.Turnout) {
        if (_clickSoundPlayer is null) {
            var audioManager = AudioManager.Current;
            _clickSoundPlayer = clickSoundType switch {
                ClickSoundTypeEnum.Turnout => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Fast.m4a").Result),
                ClickSoundTypeEnum.Button  => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result),
                _                          => audioManager.CreatePlayer(FileSystem.OpenAppPackageFileAsync("Button_Click_Mouse.m4a").Result),
            };
            _clickSoundPlayer?.Play();
            _clickSoundPlayer = null;
        }
    }

    public void RotateLeft() {
        TrackRotation -= RotationIncrement;
        if (TrackRotation < 0) TrackRotation += 360;
        InvalidateView();
    }

    public void RotateRight() {
        TrackRotation += RotationIncrement;
        if (TrackRotation > 360) TrackRotation -= 360;
        InvalidateView();
    }

    protected void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected void AddImageSourceAndRotation(TrackStyleImageEnum trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected T Clone<T>(Panel parent) where T : ITrack, new() {
        var original = JsonSerializer.Serialize(this, typeof(T), SettingsService.PanelSerializerOptions);
        var copy = JsonSerializer.Deserialize<T>(original, SettingsService.PanelSerializerOptions);

        if (copy is { } clone) {
            clone.UniqueID = Guid.NewGuid();
            clone.Parent = parent;
            return clone;
        }

        // If we could not clone the object, then just return a new instance of the 
        // object type ensuring it is part of the current Panel. 
        // ------------------------------------------------------------------------
        return new T {
            UniqueID = Guid.NewGuid(),
            Parent = parent
        };
    }
}

public enum TrackHighlightFlag  {
    None,
    Selected,
    Error,
    Path,
    Occupied,
    DragInvalid,
    DropValid
}

public enum ClickSoundTypeEnum {
    Turnout,
    Button
}