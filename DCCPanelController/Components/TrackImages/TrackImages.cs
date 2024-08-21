using DCCPanelController.Components.TrackImages;
using DCCPanelController.Components.Tracks.SVGManager;

namespace DCCPanelController.Components.Tracks;

/// <summary>
/// Track Images is a helper class that provides information about each image that is contained in the resource file
/// </summary>
public static class TrackImages {

    private static readonly Dictionary<string,TrackImageFile> AvailableImages = [];
    private static readonly object LockObject = new();

    public static Dictionary<string,TrackImageFile> AvailableTracks => AvailableImages.Count != 0 ? AvailableImages : InitializeTracks();

    public static Components.TrackImages.TrackImage? Create(string name) {
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
        Add("********","Button"                                         ,"Track_Button");
        Add("********","Label"                                          ,"Track_Label");
        Add("********","Compass"                                        ,"Track_Compass");
        
        Add("**S***S*","Track (Straight)"                               ,"Track_Straight");
        Add("**C***S*","Track Page Continuation Arrow (Straight)"       ,"Track_Straight_Continuation_Arrow");
        Add("**C***S*","Track Page Continuation Lines (Straight)"       ,"Track_Straight_Continuation_Lines");
        Add("S*S*S*S*","Track Crossing (Straight)"                      ,"Track_Straight_Cross");
        Add("**T***S*","Track Terminator (Straight)"                    ,"Track_Straight_Terminator");
       
        Add("*S***S**","Track (Angle)"                                  ,"Track_Angle");
        Add("*C***S**","Track Page Continuation Arrow (Angle)"          ,"Track_Angle_Continuation_Arrow");
        Add("*C***S**","Track Page Continuation Lines (Angle)"          ,"Track_Angle_Continuation_Lines");
        Add("*S*S*S*S","Track Crossing (Angle)"                         ,"Track_Angle_Cross");
        Add("*T***S**","Track Terminator (Angle)"                       ,"Track_Angle_Terminator");
        
        Add("*S****S*","Track Corner (Left)"                            ,"Track_Corner_Left");
        Add("*C****S*","Track Corner Page Continuation Arrow (Left)"    ,"Track_Corner_Left_Continuation_Arrow");
        Add("*C****S*","Track Corner Page Continuation Lines (Left)"    ,"Track_Corner_Left_Continuation_Lines");

        Add("***S**S*","Track Corner (Right)","Track_Corner_Right");
        Add("***C**S*","Track Corner Page Continuation Arrow (Right)"   ,"Track_Corner_Right_Continuation_Arrow");
        Add("***C**S*","Track Corner Page Continuation Lines (Right)"   ,"Track_Corner_Right_Continuation_Lines");
        
        Add("*SSS**S*","Track Threeway"                                 ,"Track_Threeway");
        Add("*DXX**S*","Track Threeway (Left)"                          ,"Track_Threeway_Left");
        Add("*XXD**S*","Track Threeway (Right)"                         ,"Track_Threeway_Right");
        Add("*XSX**S*","Track Threeway (Straight)"                      ,"Track_Threeway_Straight");

        Add("*DS***S*","Track Turnout (Left)"                           ,"Track_Turnout_Left");
        Add("*DX***S*","Track Turnout (Left) Diverging"                 ,"Track_Turnout_Left_Diverging");
        Add("*XS***S*","Track Turnout (Left) Straight"                  ,"Track_Turnout_Left_Straight");

        Add("**SD**S*","Track Turnout (Right)"                          ,"Track_Turnout_Right");
        Add("**XD**S*","Track Turnout (Right) Diverging"                ,"Track_Turnout_Right_Diverging");
        Add("**SX**S*","Track Turnout (Right) Straight"                 ,"Track_Turnout_Right_Straight");
    }

    private static void Add(string directions, string name, string reference) {
        try {
            var fullPath = SvgImageFinder.GetFullPathOfResource(reference);
            if (!string.IsNullOrEmpty(fullPath)) {
                AvailableImages.Add(reference, new TrackImageFile(name, fullPath, directions));
            }
        } catch (Exception ex) {
            Console.WriteLine($"Could not find {reference} in the resources.");
        }
    } 
}

public class TrackImageFile(string name, string svgFilename, string directions) {
    public string Name { get; set; } = name;
    public string SvgFilename { get; set; } = svgFilename;
    public TrackConnections Connections { get; } = new TrackConnections(directions);
    public Components.TrackImages.TrackImage Create => new Components.TrackImages.TrackImage(Name,SvgFilename,0,Connections);    
}

