using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Components.TrackImages;

namespace DCCPanelController.Components.TrackPieces;

public abstract partial class TrackPiece  : ObservableObject, ITrackPiece {

    protected TrackPiece() {
        Initialise();
    }
    
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _style;
    [ObservableProperty] private TrackState _state = TrackState.Unknown;
    [ObservableProperty] private int _rotation;
    [ObservableProperty] private int _xCoordinate;
    [ObservableProperty] private int _yCoordinate;
    [ObservableProperty] private ImageSource _image;

    protected int _points = 8;          // The number of compass points supports (4 or 8 generally)
    protected int _activeRotation = 0;
    protected int[] _rotationMatrix = { 0, 90, 180, 270 };
    protected readonly List<TrackImage> _tracks = [];
    
    protected abstract void Setup();

    protected int LoadedImages => _tracks.Count;
    public void RotateLeft() => RotateRelative(-(360 / _points));
    public void RotateRight() => RotateRelative(360 / _points);

    /// <summary>
    /// Used to initialise the instance of this class and setup the parameters for any derived instances
    /// </summary>
    public void Initialise() {
        Setup();
    }

    /// <summary>
    /// Rotate the image to an absolute compass point but take into account then number of points that
    /// this Track Piece supports (most support either 8 or 4)
    /// </summary>
    /// <param name="rotation">The rotation amount in absolute terms between 0 and 360</param>
    public void RotateAbsolute(int rotation) {
        _activeRotation = (int)(rotation/(360/_points))*(360/_points);
        if (_activeRotation is <= 0 or >= 360 ) _activeRotation = 0;
        SetActiveImage();
    }

    /// <summary>
    /// Rotate the image relative to the current rotation. So this ADDs or SUBs a compass offet but takes
    /// into account the number of points allows. So if we only ask to rotate by 45 and we only support 90 then
    /// it will round UP to rotate by 90. 
    /// </summary>
    /// <param name="rotation">The number of degrees to rotate the image by</param>
    public void RotateRelative(int rotation) {
        if (rotation is > 0 && rotation < (int)(360/_points)) rotation = (int)(360/_points);
        _activeRotation += (int)(rotation/(360/_points))*(360/_points);
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
        var imageNo = AdjustImageNoByState(rotationPoint % LoadedImages);
        if (imageNo >= LoadedImages) {
            Console.WriteLine($"Invalid image number {imageNo}");
            imageNo = 0;
        }
        return (imageNo, rotationPoint);
    }

    protected int AdjustImageNoByState(int imageNo) {
        return imageNo * State switch {
            TrackState.Unknown        => 1,
            TrackState.Normal         => 1,
            TrackState.Straight       => 2,
            TrackState.Diverging      => 3,
            TrackState.DivergingLeft  => 3,
            TrackState.DivergingRight => 4,
            _                         => 1
        };
    }
    
    /// <summary>
    /// Set up the Active Image. For example, as a straight image, if it is in position 0 then it is straight.
    /// If we rotate +45 then it is the Angle image at 180 rotation
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    protected void SetActiveImage() {
        try {
            var imageReference = GetImageReference();
            if (imageReference.imageNo >= LoadedImages) throw new NullReferenceException("Image number calculated is invalid.");
            Image = _tracks[imageReference.imageNo].Image ?? throw new NullReferenceException("Referenced an invalid Image.");
            Rotation = _rotationMatrix[imageReference.compassPoint % (_points / LoadedImages)];
        } catch (Exception ex) {
            Console.WriteLine($"Should not be here: {ex.Message}");
            Rotation = 0;
        }
    } 
    
    /// <summary>
    /// Helper to add a TrackImage to the collection of images. 
    /// </summary>
    protected void AddTrackImage(string imageName) => AddTrackImage(TrackImages.TrackImages.Create(imageName));
    protected void AddTrackImage(TrackImage? trackImage) {
        if (trackImage != null) _tracks.Add(trackImage);
    }
}

public enum TrackState {
    Normal,
    Straight, 
    Diverging, 
    DivergingLeft,
    DivergingRight,
    Unknown
}