using System.Diagnostics;

namespace DCCPanelController.Tracks.ImageManager;

/// <summary>
/// Track Images is a helper class that provides information about each image that is contained in the resource file
/// </summary>
public static class SvgImages {

    private static readonly Lock LockObject = new();
    private static readonly Dictionary<string, SvgImage> AvailableImages = [];
    // ReSharper disable InconsistentlySynchronizedField
    private static Dictionary<string, SvgImage> TrackImages => AvailableImages.Count != 0 ? AvailableImages : BuildTrackList();

    public static SvgImage Default() => Create("Unknown", "********", "Track_Unknown");
    public static SvgImage GetImage(string id) {
        return TrackImages.TryGetValue(id, out var trackImage) ? trackImage : Default();
    }
    
    /// <summary>
    /// Add all the types of images that we currently support. Each image has a readable name and also a string
    /// that represents the in and out paths that the track can support. 
    /// </summary>
    private static Dictionary<string, SvgImage> BuildTrackList() {
        lock (LockObject) {
            AvailableImages.Clear();
            
            Add("Unknown", "********", "Track_Unknown");

            Add("Button", "********", "Track_Button");
            Add("Label", "********", "Track_Label");
            Add("Image", "********", "Track_Image");
            Add("Compass", "********", "Track_Compass");
            Add("Points", "********", "Track_Points");

            Add("Straight1", "**S***S*", "Track_Straight");
            Add("Straight2", "*S***S**", "Track_Angle");

            Add("Terminator1", "**T***S*", "Track_Straight_Terminator");
            Add("Terminator2", "*T***S**", "Track_Angle_Terminator");

            Add("ContinuationSA1", "**C***S*", "Track_Straight_Continuation_Arrow");
            Add("ContinuationSA2", "*C***S**", "Track_Angle_Continuation_Arrow");

            Add("ContinuationSL1", "**C***S*", "Track_Straight_Continuation_Lines");
            Add("ContinuationSL2", "*C***S**", "Track_Angle_Continuation_Lines");

            Add("Cross1", "S*S*S*S*", "Track_Straight_Cross");
            Add("Cross2", "*S*S*S*S", "Track_Angle_Cross");

            Add("CornerL", "*S****S*", "Track_Corner_Left");
            Add("CornerR", "***S**S*", "Track_Corner_Right");

            Add("ContinuationCA1", "*C****S*", "Track_Corner_Left_Continuation_Arrow");
            Add("ContinuationCA2", "***C**S*", "Track_Corner_Right_Continuation_Arrow");

            Add("ContinuationCL1", "*C****S*", "Track_Corner_Left_Continuation_Lines");
            Add("ContinuationCL2", "***C**S*", "Track_Corner_Right_Continuation_Lines");

            Add("TurnoutL1", "*DS***S*", "Track_Turnout_Left");
            Add("TurnoutL2", "*DX***S*", "Track_Turnout_Left_Diverging");
            Add("TurnoutL3", "*XS***S*", "Track_Turnout_Left_Straight");

            Add("TurnoutR1", "**SD**S*", "Track_Turnout_Right");
            Add("TurnoutR2", "**XD**S*", "Track_Turnout_Right_Diverging");
            Add("TurnoutR3", "**SX**S*", "Track_Turnout_Right_Straight");

            Add("Threeway1", "*DSD**S*", "Track_Threeway");
            Add("Threeway2", "*DXX**S*", "Track_Threeway_Left");
            Add("Threeway3", "*XXD**S*", "Track_Threeway_Right");
            Add("Threeway4", "*XSX**S*", "Track_Threeway_Straight");

            return AvailableImages;
        }
    }

    private static void Add(string id, string directions, string resourceName) {
        var image = Create(id, directions, resourceName);
        AvailableImages.TryAdd(id, image);
    }

    private static SvgImage Create(string id, string directions, string resourceName) {
        var fullPath = SvgImageFinder.GetFullPathOfResource(resourceName);
        if (!string.IsNullOrEmpty(fullPath)) return new SvgImage(id, fullPath, directions);
        throw new ApplicationException($"Invalid Track Image resourceName: {resourceName}. Track name wrong or not included in project.");
    }
}
