using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.Base;

public abstract partial class TrackPiece  : BaseViewModel, ITrackPiece {

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

    [ObservableProperty] private int _imageRotation; 
    [ObservableProperty] private int _trackDirection;
    [ObservableProperty] private int _x;                    // What Grid Position (Horizontal) is this component?
    [ObservableProperty] private int _y;                    // What Grid Position (Vertical) is this component?
    [ObservableProperty] private int _width  = 1;           // How Width is it (normally 1, Text might be 2)
    [ObservableProperty] private int _height = 1;           // How High is it (Normally 1, Text might be 2)
    [ObservableProperty] private int _layer  = 1;           // What layer is this on? Only 1 element per layer.
    [ObservableProperty] private bool _isOccupied = false;  // Is this element currently occupied?
    [ObservableProperty] private bool _isResizable = false; // Can this element be resized? Normally False
    [ObservableProperty] private string? _style;
    [ObservableProperty] private string _name = "Track";

    public ImageSource?   Image => ActiveImage?.ImageSource?.Image;
    protected TrackImage? ActiveImage = null;
    protected readonly TrackImages TrackImages = new TrackImages();
    protected readonly TrackState State = new TrackState();

    public SvgCompass Connections => ActiveImage?.ImageSource?.Connections ?? new SvgCompass("********");

    /// <summary>
    /// Indicates f this element can be rotated. 
    /// </summary>
    public bool CanRotate => TrackImages.RotateBy > 1;
    
    public void RotateLeft() {
        TrackDirection = Compass.ToCompass(TrackDirection).Prev(TrackImages.RotateBy).ToRotation();
        SetActiveImage();
    }

    public void RotateRight() {
        TrackDirection = Compass.ToCompass(TrackDirection).Next(TrackImages.RotateBy).ToRotation();
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
        State.Next();
        SetActiveImage();
    }

    public void PrevState() {
        State.Prev();
        SetActiveImage();
    }

    protected abstract void Setup();

    /// <summary>
    /// Used to initialise the instance of this class and setup the parameters for any derived instances
    /// </summary>
    protected void Initialise() {
        Setup();
        State.First();
        SetActiveImage();
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
            ActiveImage = TrackImages.Get(TrackDirection, State);
            (ActiveImage?.ImageSource).ApplyStyle((string)Style);
            ActiveImage?.ImageSource.SetOccupied(IsOccupied);
            ActiveImage?.ImageSource.ForceImageRefresh();
            ImageRotation = ActiveImage?.Rotation ?? 0;
            
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
    /// <param name="trackRotation">What is the rotation of the track piece</param>
    /// <param name="trackState">What state is this image related to</param>
    /// <param name="imageSource">What is the source of this image</param>
    /// <param name="imageRotation">What rotation should this image be at for this state and track rotation</param>
    protected void AddTrackImage(int trackRotation, string trackState, string imageSource, int imageRotation) {
        State.AddState(trackState);
        TrackImages.Add(trackRotation, trackState, imageSource, imageRotation);
    }

}
