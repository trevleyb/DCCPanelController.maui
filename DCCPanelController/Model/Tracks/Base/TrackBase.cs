using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Compass = DCCPanelController.Helpers.Compass;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackBase : ObservableObject, ICloneable {
    protected readonly StyleTrackImages StyleTrackImages = new();
    
    [ObservableProperty] private string _name = "Track Piece"; // Name of this particular track piece or object
    [ObservableProperty] private int _height = 1;              // How High is it (Normally 1, Text might be 2)
    [ObservableProperty] private int _layer = 1;               // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private int _trackRotation;           // What is the expected direction of the Track Piece
    [ObservableProperty] private int _width = 1;                // How Width is it (normally 1, Text might be 2)
    [ObservableProperty] private int _x;                        // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                        // What Grid Position (Vertical) is this component?
    
    [JsonIgnore] [ObservableProperty] private int _imageRotation;
    [JsonIgnore] [ObservableProperty] private bool _isSelected;
    [JsonIgnore] protected abstract SvgImage ActiveImage { get; }
    [JsonIgnore] protected abstract SvgImage SymbolImage { get; }
    [JsonIgnore] public ImageSource Image => ActiveImage.Image ?? throw new ApplicationException("Unable to set the image");
    [JsonIgnore] public ImageSource Symbol => SymbolImage.Image ?? throw new ApplicationException("Unable to set the symbol");
    [JsonIgnore] public TrackConnectionsEnum[] Connections => ActiveImage.Connections.ConnectionPointsRotated(ImageRotation);

    private Panel? _parent;

    [JsonIgnore] public Panel? Parent {
        get {
            Console.WriteLine($"GET:Track:{Name} Parent: {_parent?.Name}");
            if (_parent is null) {
                Console.WriteLine("Null");
            }
            return _parent;
        }
        set {
            if (value == null) {
                Console.WriteLine("Set Null");
            }
            _parent = value;
            Console.WriteLine($"SET:Track:{Name} Parent: {_parent?.Name}");
        }
    }

    protected TrackBase() {
        Initialise();
        OnPropertyChanged(nameof(Image));
        PropertyChanged += OnPropertyChanged;
    }

    public virtual string TrackObjectType {
        get => GetType().Name;
        set => _ = value;
    }
    
    /// <summary>
    ///     Used to initialise the instance of this class and set up the parameters for any derived instances
    /// </summary>
    protected void Initialise() {
        Setup();
    }

    protected abstract void Setup();

    /// <summary>
    ///     Manage when properties have changed as we may need to redraw the image
    /// </summary>
    private static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    public void RotateLeft() {
        TrackRotation = Compass.ToCompass(TrackRotation).Prev().ToRotation();
        OnPropertyChanged(nameof(Image));
    }

    public void RotateRight() {
        TrackRotation = Compass.ToCompass(TrackRotation).Next().ToRotation();
        OnPropertyChanged(nameof(Image));
    }

    protected void SetTrackSymbol(string imageSource) {
        AddImageSourceAndRotation(TrackStyleImage.Symbol, imageSource, (0, 0), (0, 0), (0, 0), (0, 0));
    }

    protected void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    protected void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        StyleTrackImages.AddImageSourceAndRotation(trackType, imageSource, rotations);
    }

    public virtual object Clone() {
        return MemberwiseClone();
    }
}