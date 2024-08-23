using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.SVGManager;

namespace DCCPanelController.Components.TrackPieces.Base;

public abstract partial class TrackPiece  : ObservableObject, ITrackPiece {

    protected TrackPiece(string name = "Track", string style = "Default", int rotation = 0, int x = 0, int y = 0) {
        Name = name;
        Style = style;
        Rotation = rotation;
        X = x;
        Y = y;
        Initialise();
        OnPropertyChanged(nameof(Image));
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _style;
    [ObservableProperty] private int _rotation;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private TrackState _state = new TrackState();

    public ImageSource? Image => _activeTrackImage?.Image;

    private int _points = 8;          // The number of compass points supports (4 or 8 generally)
    private int _activeRotation = 0;
    private int[] _rotationMatrix = { 0, 90, 180, 270 };
    private SvgImage? _activeTrackImage = null;
    private readonly List<SvgImage> Tracks = [];
    
    protected abstract void Setup();

    protected int LoadedImages => Tracks.Count;
    public void RotateLeft() => RotateRelative(-(360 / _points));
    public void RotateRight() => RotateRelative(360 / _points);
    public void NextState() => State.Next();
    public void PrevState() => State.Prev();
    
    /// <summary>
    /// Used to initialise the instance of this class and setup the parameters for any derived instances
    /// </summary>
    protected void Initialise() {
        Setup();
    }

    protected void SetRotationMatrix(int[] rotationMatrix) => SetRotationMatrix(rotationMatrix.Length, rotationMatrix);
    protected void SetRotationMatrix(int points, int[] rotationMatrix) {
        _points = points;
        _rotationMatrix = rotationMatrix;
    }
    
    /// <summary>
    /// Rotate the image to an absolute compass point but take into account then number of points that
    /// this Track Piece supports (most support either 8 or 4)
    /// </summary>
    /// <param name="degrees">The rotation amount in absolute terms between 0 and 360</param>
    public void RotateAbsolute(int degrees) {
        _activeRotation = (int)(degrees/(360/_points))*(360/_points);
        if (_activeRotation is <= 0 or >= 360 ) _activeRotation = 0;
        SetActiveImage();
    }

    /// <summary>
    /// Rotate the image relative to the current rotation. So this ADDs or SUBs a compass offet but takes
    /// into account the number of points allows. So if we only ask to rotate by 45 and we only support 90 then
    /// it will round UP to rotate by 90. 
    /// </summary>
    /// <param name="degrees">The number of degrees to rotate the image by</param>
    public void RotateRelative(int degrees) {
        if (degrees is > 0 && degrees < (int)(360/_points)) degrees = (int)(360/_points);
        _activeRotation += (int)(degrees/(360/_points))*(360/_points);
        while (_activeRotation is < 0 or > 360) {
            if (_activeRotation < 0) _activeRotation += 360;
            if (_activeRotation > 360) _activeRotation -= 360;
        }
        SetActiveImage();
    }

    /// <summary>
    /// Calculates which image should be displayed and what the compass point is as this determines the rotation
    /// of the image. This will potentially vary by implementation
    /// </summary>
    protected (int imageNo, int compassPoint) GetImageReference() {
        var rotationPoint = ((int)(_activeRotation / (360 / _points)));
        var imageNo = (rotationPoint % LoadedImages) * State.Offset;
        if (imageNo >= LoadedImages) {
            Console.WriteLine($"Invalid image number {imageNo}");
            imageNo = 0;
        }
        return (imageNo, rotationPoint);
    }
    
    /// <summary>
    /// Set up the Active Image. For example, as a straight image, if it is in position 0 then it is straight.
    /// If we rotate +45, then it is the Angle image at 180 rotation
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    protected void SetActiveImage() {
        try {
            var imageReference = GetImageReference();
            if (imageReference.imageNo >= LoadedImages) throw new NullReferenceException("Image number calculated is invalid.");
            _activeTrackImage = Tracks[imageReference.imageNo] ?? throw new NullReferenceException("Referenced an invalid Image.");
            OnPropertyChanged(nameof(Image));
            Rotation = _rotationMatrix[imageReference.compassPoint % (_points / LoadedImages)];
        } catch (Exception ex) {
            Console.WriteLine($"Should not be here: {ex.Message}");
            Rotation = 0;
        }
    } 
    
    /// <summary>
    /// Helper to add a TrackImage to the collection of images. 
    /// </summary>
    protected void AddTrackImage(string imageName) => AddTrackImage(SvgImages.Create(imageName));
    protected void AddTrackImage(SvgImage? trackImage) {
        if (trackImage != null) Tracks.Add(trackImage);
    }
}
