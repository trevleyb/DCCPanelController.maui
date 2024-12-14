using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using Compass = DCCPanelController.Helpers.Compass;

namespace DCCPanelController.Model.Tracks.Base;

public abstract partial class TrackBase : ObservableObject {

    protected TrackBase(Panel? parent = null) {
        Initialise();
        OnPropertyChanged(nameof(DisplayImage));
        PropertyChanged += OnPropertyChanged;
        if (parent != null) Parent = parent;
    }
    
    protected readonly StyleTrackImages StyleTrackImages = new();

    [ObservableProperty] private string _name = "Track Piece"; // Name of this particular track piece or object
    [ObservableProperty] private int _layer = 1;               // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private int _trackRotation;           // What is the expected direction of the Track Piece
    [ObservableProperty] private int _x;                       // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                       // What Grid Position (Vertical) is this component?
    [ObservableProperty] [property: JsonIgnore] private bool _isSelected;

    [JsonIgnore] public TrackConnectionsEnum[] Connections => ActiveImage.Connections.ConnectionPointsRotated(ImageRotation);
    [JsonIgnore] public ImageSource DisplayImage => ActiveImage.Image ?? throw new ApplicationException("Unable to set the image");
    [JsonIgnore] public ImageSource DisplaySymbol => SymbolImage.Image ?? throw new ApplicationException("Unable to set the symbol");
    [JsonIgnore] public double GridSize;             
    [JsonIgnore] public int ImageRotation;
    [JsonIgnore] protected abstract SvgImage ActiveImage { get; }
    [JsonIgnore] protected abstract SvgImage SymbolImage { get; }
    
    [DoNotClone][JsonIgnore] public Panel? Parent { get; set; }
   
    public IView GetDisplayItem(double gridSize, bool passthrough = false ) {
        GridSize = gridSize;
        var image = new Image {
            Source = DisplayImage,
            Scale = 1.5,
            ZIndex = Layer,
            Rotation = 0,
            WidthRequest = gridSize,
            HeightRequest = gridSize,
            RotationX = ImageRotation,
            InputTransparent = passthrough,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };

        // Setup bindings to the size and source of the Track DisplayImage. DisplayImage can change on events
        // -------------------------------------------------------------------------------------------
        //image.SetBinding(Image.SourceProperty, new Binding(nameof(DisplayImage)) { Source = this });
        //image.SetBinding(VisualElement.RotationProperty, new Binding(nameof(ImageRotation)) { Source = this });
        //image.SetBinding(VisualElement.WidthRequestProperty, new Binding(nameof(GridSize)) { Source = this });
        //image.SetBinding(VisualElement.HeightRequestProperty, new Binding(nameof(GridSize)) { Source = this });
        return image;
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
        OnPropertyChanged(nameof(DisplayImage));
    }

    public void RotateRight() {
        TrackRotation = Compass.ToCompass(TrackRotation).Next().ToRotation();
        OnPropertyChanged(nameof(DisplayImage));
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

    public abstract ITrackPiece Clone();
}