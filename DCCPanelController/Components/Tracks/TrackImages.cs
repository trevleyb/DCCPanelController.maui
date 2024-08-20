using DCCPanelController.Components.Tracks.SVGManager;

namespace DCCPanelController.Components.Tracks;

/// <summary>
/// Track Images is a helper class that provides information about each image that is contained in the resource file
/// </summary>
public static class TrackImages {

    private static readonly Dictionary<string,TrackImageFile> AvailableImages = [];
    private static readonly object LockObject = new();

    public static Dictionary<string,TrackImageFile> AvailableTracks => AvailableImages.Count != 0 ? AvailableImages : InitializeTracks();

    public static TrackImage? Create(string name) {
        if (AvailableTracks.ContainsKey(name)) return AvailableTracks[name].Create;
        var closestMatch = AvailableTracks.Keys.FirstOrDefault(key => key.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (closestMatch != null) return AvailableTracks[closestMatch].Create;
        return null;    
    }

    private static Dictionary<string, TrackImageFile> InitializeTracks() {
        lock (LockObject) {
            if (AvailableImages.Count == 0) AddTrackImages();
            return AvailableImages;
        }
    }
    
    /// <summary>
    /// Add all the types of images that we currently support. Each image has a readable name and also a string
    /// that represents the in and out paths that the track can support. 
    /// </summary>
    private static void AddTrackImages() {
        Add("*S***S**","Track Angle","Track_Angle");
        Add("*S***S**","Track Angle with Button","Track_Angle_Button");
        Add("*S***S**","Track Angle with Button (Short Side)","Track_Angle_Button_Short");
        Add("*T***S**","Track Angle - Terminated (Long)","Track_Angle_Terminator_Long");
        Add("*T***S**","Track Angle - Terminated (Short)","Track_Angle_Terminator_Short");
        Add("*T***S**","Track Angle - Terminated (Lines)","Track_Angle_Terminator_Short_Lines");
        Add("**C***S*","Track Page Continuation (Arrow)","Track_Continuation_Arrow");
        Add("**C***S*","Track Page Continuation (Lines)","Track_Continuation_Lines");
        Add("*S****S*","Track Corner (Left)","Track_Corner_Left");
        Add("*S****S*","Track Corner (Left) with Button","Track_Corner_Left_Button");
        Add("*S****S*","Track Corner (Left) with Button (Long Side)","Track_Corner_Left_Button_Long");
        Add("*S****S*","Track Corner (Left) with Button (Short Side)","Track_Corner_Left_Button_Short");
        Add("*C****S*","Track Corner (Left) Page Continuation (Arrow)","Track_Corner_Left_Continuation_Arrow");
        Add("*C****S*","Track Corner (Left) Page Continuation (Lines)","Track_Corner_Left_Continuation_Lines");
        Add("***S**S","Track Corner (Right)","Track_Corner_Right");
        Add("***S**S","Track Corner (Right) with Button","Track_Corner_Right_Button");
        Add("***S**S","Track Corner (Right) with Button (Long Side)","Track_Corner_Right_Button_Long");
        Add("***S**S","Track Corner (Right) with Button (Short Side)","Track_Corner_Right_Button_Short");
        Add("***C**S","Track Corner (Right) Page Continuation (Arrow)","Track_Corner_Right_Continuation_Arrow");
        Add("***C**S","Track Corner (Right) Page Continuation (Lines)","Track_Corner_Right_Continuation_Lines");
        Add("*S*S*S*S","Track Crossing (Angle)","Track_Crossing_Angle");
        Add("S*S*S*S*","Track Crossing (Straight)","Track_Crossing_Straight");
        Add("**S***S*","Track Straight","Track_Straight");
        Add("**S***S*","Track Straight with Button","Track_Straight_Button");
        Add("**T***S*","Track Terminator","Track_Terminator");
        Add("*SSS**S*","Track Threeway","Track_Threeway");
        Add("*DXX**S*","Track Threeway (Left)","Track_Threeway_Left");
        Add("*XXD**S*","Track Threeway (Right)","Track_Threeway_Right");
        Add("*XSX**S*","Track Threeway (Straight)","Track_Threeway_Straight");
        Add("*DS***S*","Track Turnout (Left)","Track_Turnout_Left");
        Add("*DX***S*","Track Turnout (Left) Diverging","Track_Turnout_Left_Diverging");
        Add("*DS***S*","Track Turnout (Left) With Label","Track_Turnout_Left_Label");
        Add("*XS***S*","Track Turnout (Left) Straight","Track_Turnout_Left_Straight");
        Add("**SD**S*","Track Turnout (Right)","Track_Turnout_Right");
        Add("**XD**S*","Track Turnout (Right) Diverging","Track_Turnout_Right_Diverging");
        Add("**SD**S*","Track Turnout (Right) With Label","Track_Turnout_Right_Label");
        Add("**SX**S*","Track Turnout (Right) Straight","Track_Turnout_Right_Straight");
    }

    private static void Add(string directions, string name, string reference) {
        var fullPath = SvgImageFinder.GetFullPathOfResource(reference);
        if (!string.IsNullOrEmpty(fullPath)) {
            AvailableImages.Add(reference, new TrackImageFile(name, fullPath, directions));
        }
    } 
}

public class TrackImageFile(string name, string svgFilename, string directions) {
    public string Name { get; set; } = name;
    public string SvgFilename { get; set; } = svgFilename;
    public TrackConnections Connections { get; } = new TrackConnections(directions);
    public TrackImage Create => new TrackImage(Name,SvgFilename,0,Connections);    
}

public class TrackConnections {
    /// <summary>
    /// Track what connections this particular component supports based on a Rotation of '0'
    /// </summary>
    /// <param name="directions">A string representing the compass Directions. Example, straight is = '**S***S*'</param>
    public TrackConnections(string directions) {
        for (var i = 0; i < Math.Min(directions.Length, 8); ++i) {
            ConnectionsArray[i] = char.ToLower(directions[i]) switch {
                't' => TrackConnectionsEnum.Terminator,
                's' => TrackConnectionsEnum.Straight,
                'd' => TrackConnectionsEnum.Diverging,
                'x' => TrackConnectionsEnum.Closed,
                _   => TrackConnectionsEnum.None
            };
        }
    }
    
    private TrackConnectionsEnum[] ConnectionsArray { get; } = new TrackConnectionsEnum[8];

    public TrackConnectionsEnum[] Connections(int rotation = 0) {

        var rotationIndex = rotation switch {
            >= 0 and <= 90    => 0,
            >= 90 and <= 180  => 1,
            >= 180 and <= 270 => 2,
            >= 270 and <= 360 => 3,
            >= 360            => 0,
            _                 => 0
        };
        
        var result = new TrackConnectionsEnum[8];
        for (var i = 0; i < 8; ++i) {
            var newIndex = (i + rotationIndex) % 4;
            result[newIndex] = ConnectionsArray[i];
        }
        return result;
    }

    public TrackConnectionsEnum Connection(TrackDirectionEnum direction, int rotation = 0) {
        var connections = Connections(rotation);
        return connections[(int)direction];
    }
}

public enum TrackDirectionEnum {
    North       = 0, 
    NorthEast   = 1, 
    East        = 2, 
    SouthEast   = 3, 
    South       = 4, 
    SouthWest   = 5, 
    West        = 6, 
    NorthWest   = 7
}

public enum TrackConnectionsEnum {
    None        = 'N',
    Terminator  = 'T', 
    Straight    = 'S',
    Closed      = 'X',
    Diverging   = 'D'
}