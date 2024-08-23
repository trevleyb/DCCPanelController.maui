using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.SVGManager;

namespace DCCPanelController.Components.TrackPieces.Base;

public abstract partial class TrackPiece  : BaseViewModel, ITrackPiece {

    protected TrackPiece(string name = "Track", string style = "Default", int rotation = 0, int x = 0, int y = 0) {
        Name = name;
        Style = style;
        TrackDirection = rotation;
        X = x;
        Y = y;
        Initialise();
        OnPropertyChanged(nameof(Image));
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _style;
    [ObservableProperty] private int _imageRotation; 
    [ObservableProperty] private int _trackDirection;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private TrackState _state = new TrackState();

    public ImageSource?   Image => ActiveImage?.ImageSource?.Image;
    protected TrackImage? ActiveImage = null;
    protected TrackImages TrackImages = new TrackImages();

    public SvgCompass Connections => ActiveImage?.ImageSource?.Connections ?? new SvgCompass("********"); 

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
            ImageRotation = ActiveImage.Value.Rotation;
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
