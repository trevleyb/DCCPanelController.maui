using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackPiece : BaseViewModel, ITrackPiece {
    public const string UnknownState = "Unknown";

    protected TrackPiece() {
        IsOccupied = false;
        Initialise();
        OnPropertyChanged(nameof(Image));
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Manage when properties have changed as we may need to redraw the image
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(IsOccupied):
            SetActiveImage();
            break;
        }
    }
    private readonly TrackImages _trackImages = new();
    private readonly TrackState _state = new();
    private readonly Dictionary<string, SvgStyle> _trackStyles = new();

    [ObservableProperty] private string _name = "Track";
    [ObservableProperty] private string _defaultState = "";
    [ObservableProperty] private int _imageRotation;
    [ObservableProperty] private int _trackDirection;
    [ObservableProperty] private int _x;                    // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                    // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _width = 1;            // How Width is it (normally 1, Text might be 2)
    [ObservableProperty] private int _height = 1;           // How High is it (Normally 1, Text might be 2)
    [ObservableProperty] private int _layer = 1;            // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private bool _isOccupied = false;  // Is this element currently occupied?
    [ObservableProperty] private bool _isResizable = false; // Can this element be resized? Normally False

    public ImageSource? Image => ActiveImage?.ImageSource?.Image;
    protected TrackImage? ActiveImage;

    public TrackConnectionsEnum[] Connections => ActiveImage?.ImageSource?.Connections.ConnectionPointsRotated(ImageRotation) ?? new SvgCompass().ConnectionPointsRotated(ImageRotation);

    /// <summary>
    /// Indicates f this element can be rotated. 
    /// </summary>
    public bool CanRotate => _trackImages.RotateBy > 1;

    public void RotateLeft() {
        TrackDirection = Compass.ToCompass(TrackDirection).Prev(_trackImages.RotateBy).ToRotation();
        SetActiveImage();
    }

    public void RotateRight() {
        TrackDirection = Compass.ToCompass(TrackDirection).Next(_trackImages.RotateBy).ToRotation();
        SetActiveImage();
    }

    public void RotateAbsolute(CompassPoints direction) {
        TrackDirection = direction.ToRotation();
        SetActiveImage();
    }

    public void RotateAbsolute(int rotation) {
        TrackDirection = rotation;
        SetActiveImage();
    }

    public void NextState() {
        _state.Next();
        SetActiveImage();
    }

    public void PrevState() {
        _state.Prev();
        SetActiveImage();
    }

    protected abstract void Setup();
    protected abstract void AddTrackImages();
    protected abstract void AddTrackStyles();

    /// <summary>
    /// Used to initialise the instance of this class and setup the parameters for any derived instances
    /// </summary>
    protected void Initialise() {
        Setup();
        AddTrackImages();
        AddTrackStyles();
        _state.First();
        SetActiveImage();
    }

    public ImageSource? SymbolImage {
        get {
            if (_trackImages.SymbolImage is { ImageSource: not null } symbolImage) {
                symbolImage.ImageSource.ApplyStyle(GetTrackStyle(DefaultState));
                symbolImage.ImageSource.SetOccupied(false);
                symbolImage.ImageSource.ForceImageRefresh();
                return symbolImage.ImageSource.Image;
            }

            return _trackImages.SymbolImage.ImageSource?.Image;
        }
    }

    /// <summary>
    /// Set up the Active Image. For example, as a straight image, if it is in position 0, then it is straight.
    /// If we rotate +45, then it is the Angle image at rotation 180
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    protected void SetActiveImage() {
        IsBusy = true;
        IsRefreshing = true;
        try {
            // Get the image that should be displayed on the screen based on the direction or rotation that we
            // have and the current state. However, the rotation of the image itself might be different so we 
            // set Rotation to the value defined against the active image. 
            // -----------------------------------------------------------------------------------------------
            ActiveImage = _trackImages.Get(TrackDirection, _state);
            if (ActiveImage is { ImageSource: not null } activeImage) {
                activeImage.ImageSource.ApplyStyle(GetTrackStyle(_state.State));
                activeImage.ImageSource.SetOccupied(IsOccupied);
                activeImage.ImageSource.ForceImageRefresh();
                ImageRotation = ActiveImage?.Rotation ?? 0;
            }

            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(ImageRotation));
        } catch (Exception ex) {
            Console.WriteLine($"Should not be here: {ex.Message}");
            ImageRotation = 0;
        }

        IsRefreshing = false;
        IsBusy = false;
    }

    /// <summary>
    /// Helper to add a Track Image to the collection of available images that could be displayed. 
    /// </summary>
    protected void AddTrackImage(int trackRotation, string trackState, string imageSource, int imageRotation) {
        _state.AddState(trackState);
        _trackImages.Add(trackRotation, trackState, imageSource, imageRotation);
    }

    protected void SetTrackSymbol(string imageSource, int rotation = 0) {
        _trackImages.SetTrackSymbol(imageSource, rotation);
    }

    protected void SetTrackStyle(string stateName, string styleName) {
        SetTrackStyle(stateName, SvgStyles.GetStyle(styleName));
    }

    protected void SetTrackStyle(string stateName, SvgStyle style) {
        if (_trackStyles.ContainsKey(stateName)) {
            _trackStyles[stateName.ToLowerInvariant()] = style;
        } else {
            AddTrackStyle(stateName.ToLowerInvariant(), style);
        }
    }

    protected void AddTrackStyle(string stateName, string styleName) {
        AddTrackStyle(stateName, SvgStyles.GetStyle(styleName.ToLowerInvariant()));
    }

    protected void AddTrackStyle(string stateName, SvgStyle style) {
        _trackStyles.TryAdd(stateName.ToLowerInvariant(), style);
    }

    protected SvgStyle GetTrackStyle(string stateName) {
        var selectedState = stateName ?? DefaultState;
        return _trackStyles.TryGetValue(selectedState.ToLowerInvariant(), out var value) ? value : SvgStyles.DefaultStyle;
    }
}