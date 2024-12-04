namespace DCCPanelController.Tracks.StyleManager;

using System.Collections.Generic;

public class StyleTrackImages {

    private int _currentState;
    private readonly List<StyleTrackImage> _images = [];
    private readonly List<TrackStyleImage> _states = [];
    public IReadOnlyList<StyleTrackImage> Images => _images.AsReadOnly();

    /// <summary>
    /// Given the type of Track we need (Unknown, Straight, Diverging, etc.) and the current rotation of the track piece,
    /// ie: how are we expecting it to look, then return the image along with how it actually should be rotated. 
    /// </summary>
    /// <param name="trackType">The type that this track is</param>
    /// <param name="trackRotation">What the current expected rotation is</param>
    /// <returns></returns>
    public (string ImageSource, int Rotation) GetTrackImageSourceAndRotation(TrackStyleImage trackType, int trackRotation) {
        // Maximum number of attempts to find the correct image by incrementing the rotation
        const int maxAttempts = 8; // This allows for searching up to a full 360 degrees
        const int rotationIncrement = 45;

        for (var attempt = 0; attempt < maxAttempts; attempt++) {
            var currentRotation = (trackRotation + (attempt * rotationIncrement)) % 360;
            var image = _images.FirstOrDefault(i => i.Image == trackType && i.Rotations.Any(r => r.TrackRotation == currentRotation));
            if (image != null) {
                var selectedRotation = image.Rotations.FirstOrDefault(r => r.TrackRotation == currentRotation);
                return (image.ImageSource, selectedRotation?.ImageRotation ?? 0);
            }
        }
        // Default if no match is found
        return ("Unknown", 0);
    }

    public (string imageSource, int rotation) GetTrackImageSourceAndRotation(int trackRotation) {
        if (_states.Count <= 0) return ("Unknown", 0);
        var trackType = _states[_currentState];
        return GetTrackImageSourceAndRotation(trackType, trackRotation);
    }

    public void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, params (int TrackRotation, int ImageRotation)[] rotations) {
        AddToStateList(trackType);
        var builder = StyleTrackImage.Create(trackType, imageSource);
        if (rotations.Length == 0) builder.AddDefaultRotations();
        if (rotations.Length > 0) builder.AddRotations(rotations);
        _images.Add(builder.Build());
    }

    public void AddImageSourceAndRotation(TrackStyleImage trackType, string imageSource, List<StyleTrackImage.Rotation> rotations) {
        AddToStateList(trackType);
        var builder = StyleTrackImage.Create(trackType, imageSource);
        builder.AddRotations(rotations);
        _images.Add(builder.Build());
    }
    
    private void AddToStateList(TrackStyleImage trackType) {
        if (!_states.Contains(trackType)) _states.Add(trackType);
    }

    private TrackStyleImage First() { return GetNextState(0, 0); }
    private TrackStyleImage Last() { return GetNextState(0, -1); }
    private TrackStyleImage Next() { return GetNextState(_currentState,  1); }
    private TrackStyleImage Prev() { return GetNextState(_currentState, -1); }
    private TrackStyleImage Next(TrackStyleImage trackType) { return GetNextState(trackType,  1); }
    private TrackStyleImage Prev(TrackStyleImage trackType) { return GetNextState(trackType, -1); }

    private TrackStyleImage GetNextState(TrackStyleImage current, int step) {
        _currentState = _states.IndexOf(current);
        return GetNextState(_currentState, step);
    }

    private TrackStyleImage GetNextState(int currentIndex, int step) {
        var nextIndex = currentIndex + step;
        var movingForward = step > 0;
        var atEndOfList = nextIndex >= _states.Count;
        var atStartOfList = nextIndex < 0;

        nextIndex = movingForward switch {
            true when atEndOfList    => 0,
            false when atStartOfList => _states.Count - 1,
            _                        => nextIndex
        };

        if (nextIndex < 0 || nextIndex >= _states.Count) {
            return TrackStyleImage.Normal;
        }

        return _states[nextIndex];
    }
}