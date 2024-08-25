namespace DCCPanelController.Tracks.Base;

public class TrackImages {
    
    public Dictionary<(CompassPoints points, string state), TrackImage> ImageRotations { get; set; } = new();

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

    public void Add(int trackRotation, string trackState, string imageSource, int rotation) {
        try {
            var svgImage = new TrackImage(imageSource, rotation);
            ImageRotations[(Compass.ConvertFromDegress(trackRotation), trackState)] = new TrackImage(imageSource, rotation);
        } catch {
            Console.WriteLine($"Failed to add track image: {imageSource} as it does not exist.");
        }
    }

    public TrackImage Get(int trackRotation, TrackState trackState) => Get(Compass.ConvertFromDegress(trackRotation), trackState.State);
    public TrackImage Get(CompassPoints compassPoint, TrackState trackState) => Get(compassPoint, trackState.State); 
    public TrackImage Get(int trackRotation, string trackState) => Get(Compass.ConvertFromDegress(trackRotation), trackState);
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

