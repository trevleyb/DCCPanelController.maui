using System.Diagnostics;

namespace DCCPanelController.Components.SVGManager;

/// <summary>
/// Track Images is a helper class that provides information about each image that is contained in the resource file
/// </summary>
public static class SvgImages {

    public static SvgImage UnknownImage => Create("Track_Unknown") ?? throw new ArgumentNullException(nameof(UnknownImage));  
    private static readonly Dictionary<string,TrackImageFile> AvailableImages = [];
    private static readonly object LockObject = new();

    public static Dictionary<string,TrackImageFile> AvailableTracks => AvailableImages.Count != 0 ? AvailableImages : InitializeTracks();

    public static SvgImage Create(string id) {
        return AvailableTracks.ContainsKey(id) ? AvailableTracks[id].Create : UnknownImage;
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
        Add("Button",           "********","Track_Button");
        Add("Label",            "********","Track_Label");
        Add("Compass",          "********","Track_Compass");
        Add("Unknown",          "********","Track_Unknown");
        
        Add("Straight1",        "**S***S*","Track_Straight");
        Add("Straight2",        "*S***S**","Track_Angle");
        
        Add("Terminator1",      "**T***S*","Track_Straight_Terminator");
        Add("Terminator2",      "*T***S**","Track_Angle_Terminator");
        
        Add("ContinuationSA1",  "**C***S*","Track_Straight_Continuation_Arrow");
        Add("ContinuationSA2",  "*C***S**","Track_Angle_Continuation_Arrow");
       
        Add("ContinuationSL1",  "**C***S*","Track_Straight_Continuation_Lines");
        Add("ContinuationSL2",  "*C***S**","Track_Angle_Continuation_Lines");

        Add("Cross1",           "S*S*S*S*","Track_Straight_Cross");
        Add("Cross2",           "*S*S*S*S","Track_Angle_Cross");
        
        Add("CornerL",          "*S****S*","Track_Corner_Left");
        Add("CornerR",          "***S**S*","Track_Corner_Right");

        Add("ContinuationCA1",  "*C****S*","Track_Corner_Left_Continuation_Arrow");
        Add("ContinuationCA2",  "***C**S*","Track_Corner_Right_Continuation_Arrow");

        Add("ContinuationCL1",  "*C****S*","Track_Corner_Left_Continuation_Lines");
        Add("ContinuationCL2",  "***C**S*","Track_Corner_Right_Continuation_Lines");

        Add("TurnoutL1",        "*DS***S*","Track_Turnout_Left");
        Add("TurnoutL2",        "*DX***S*","Track_Turnout_Left_Diverging");
        Add("TurnoutL3",        "*XS***S*","Track_Turnout_Left_Straight");

        Add("TurnoutR1",        "**SD**S*","Track_Turnout_Right");
        Add("TurnoutR2",        "**XD**S*","Track_Turnout_Right_Diverging");
        Add("TurnoutR3",        "**SX**S*","Track_Turnout_Right_Straight");
        
        Add("Threeway1",        "*SSS**S*","Track_Threeway");
        Add("Threeway2",        "*DXX**S*","Track_Threeway_Left");
        Add("Threeway3",        "*XXD**S*","Track_Threeway_Right");
        Add("Threeway4",        "*XSX**S*","Track_Threeway_Straight");
        
    }

    private static void Add(string id, string directions, string reference) {
        try {
            var fullPath = SvgImageFinder.GetFullPathOfResource(reference);
            if (!string.IsNullOrEmpty(fullPath)) {
                AvailableImages.Add(id, new TrackImageFile(id, fullPath, directions));
            }
        } catch (Exception ex) {
            Console.WriteLine($"Could not find {reference} in the resources.");
        }
    } 
}

public enum TrackViewModelType {
    Straight,
    Terminator,
    Corner, 
    StraightContinuation,
    CornerContinuation,
    Crossing, 
    LeftTurnout,
    RightTurnout,
    Threeway
}

[DebuggerDisplay("{Id}")]
public class TrackImageFile(string id, string svgFilename, string directions) {
    public string Id { get; set; } = id;
    public string SvgFilename { get; set; } = svgFilename;
    public SvgCompass Connections { get; } = new SvgCompass(directions);
    public SvgImage Create => new SvgImage(id, SvgFilename,0,Connections);    
}

