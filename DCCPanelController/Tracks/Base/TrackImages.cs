namespace DCCPanelController.Tracks.Base;

public class TrackImages {
    private TrackImage? _symbolImage;
    public Dictionary<(CompassPoints points, string state), TrackImage> ImageRotations { get; } = new();
    public TrackImage SymbolImage => _symbolImage ?? new TrackImage("Track_Unknown", 0);

    /// <summary>
    /// Return the number of defined points. Sometimes we rotate only 4 points of the compass and sometimes 8,
    /// and it is dependent on the number of defined points. 
    /// </summary>
    public int NumberOfPoints {
        get {
            var points = ImageRotations.Select(item => item.Key.points).Distinct().Count();
            return points is > 0 and < 8 ? points : 8;
        }
    }

    public int RotateBy => 8 / NumberOfPoints;

    /// <summary>
    /// Set the track image that will be used as a Symbol in the available
    /// symbols list.  
    /// </summary>
    public void SetTrackSymbol(string imageSource, int rotation = 0) {
        _symbolImage = new TrackImage(imageSource, rotation);
    }

    public void Add(int trackRotation, string trackState, string imageSource, int rotation) {
        try {
            var svgImage = new TrackImage(imageSource, rotation);
            ImageRotations[(Compass.ConvertFromDegress(trackRotation), trackState)] = svgImage;
        } catch {
            Console.WriteLine($"Failed to add track image: {imageSource} as it does not exist.");
        }
    }

    public TrackImage Get(int trackRotation, TrackState trackState) {
        return Get(Compass.ConvertFromDegress(trackRotation), trackState.State);
    }

    public TrackImage Get(CompassPoints compassPoint, TrackState trackState) {
        return Get(compassPoint, trackState.State);
    }

    public TrackImage Get(int trackRotation, string trackState) {
        return Get(Compass.ConvertFromDegress(trackRotation), trackState);
    }

    public TrackImage Get(CompassPoints compassPoint, string trackState) {
        var attempts = 0;
        while (attempts < 8) {
            if (ImageRotations.TryGetValue((compassPoint, trackState), out var image)) return image;
            compassPoint.Next(1);
            attempts++;
        }

        throw new Exception("Could not find track image");
    }
}