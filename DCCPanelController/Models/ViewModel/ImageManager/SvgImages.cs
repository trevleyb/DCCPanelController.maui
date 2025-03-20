using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgImages {
    public static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
    private record SvgReference(string Filename, int Rotation, SvgConnections Connections);

    static SvgImages() {
        AvailableSymbols = BuildAvailableSymbols();
        using (new CodeTimer("Building Image Cache")) {
            BuildAvailableImages();
        }
    }

    private static readonly Lock LockObject = new();
    private static readonly List<string> AvailableSymbols;
    private static readonly Dictionary<string, Dictionary<int, SvgReference>> Images = new();
    
    private static void BuildAvailableImages() {
        Console.WriteLine("Building Available Images");
        AddImage("Unknown", "Track_Unknown");

        AddImage("Text", "Track_Text");
        AddImage("Label", "Track_Label");
        AddImage("Image", "Track_Image");
        AddImage("Compass", "Track_Compass");
        AddImage("Points", "Track_Points");
        AddImage("Circle", "draw_circle");
        AddImage("Rectangle", "draw_rectangle");
        AddImage("Line", "draw_line");

        AddImage("Button", "Track_Button", 4);                        // Add Buttons at N,E,S,W
        AddImage("Button", "Track_Button_Corner", "********", 4, 45); // Add Buttons at NE,SE,SW,NW
        AddImage("ButtonLarge", "Track_Large_Button", 4);             // Add Buttons at N,E,S,W

        AddImage("Platform", "Track_Platform", "**S***S*", 4);
        AddImage("Straight", "Track_Straight", "**S***S*", 4);
        AddImage("Straight", "Track_Angle", "*S***S**", 4, 45);

        AddImage("Cross", "Track_Straight_Cross", "S*S*S*S*", 4);
        AddImage("Cross", "Track_Angle_Cross", "*S*S*S*S", 4, 45);

        AddImage("Corner", "Track_Corner_Left", "*S****S*", 4);
        AddImage("Corner", "Track_Corner_Right", "***S**S*", 4, 45);

        AddImage("Terminator", "Track_Straight_Terminator", "**T***S*", 4);
        AddImage("Terminator", "Track_Angle_Terminator", "*T***S**", 4, 45);

        AddImage("StraightContinuationArrow", "Track_Straight_Continuation_Arrow", "**C***S*", 4);
        AddImage("StraightContinuationArrow", "Track_Angle_Continuation_Arrow", "*C***S**", 4, 45);
        AddImage("StraightContinuationLines", "Track_Straight_Continuation_Lines", "**C***S*", 4);
        AddImage("StraightContinuationLines", "Track_Angle_Continuation_Lines", "*C***S**", 4, 45);

        AddImage("CornerContinuationArrow", "Track_Corner_Left_Continuation_Arrow", "*C****S*", 4);
        AddImage("CornerContinuationArrow", "Track_Corner_Right_Continuation_Arrow", "***C**S*", 4, 45);
        AddImage("CornerContinuationLines", "Track_Corner_Left_Continuation_Lines", "*C****S*", 4);
        AddImage("CornerContinuationLines", "Track_Corner_Right_Continuation_Lines", "***C**S*", 4, 45);

        AddImage("LeftTurnoutUnknown", "Track_Turnout_Left", "*DX***S*"); // Support all 8 rotations
        AddImage("LeftTurnoutStraight", "Track_Turnout_Left_Straight", "*DX***S*");
        AddImage("LeftTurnoutDiverging", "Track_Turnout_Left_Diverging", "*DX***S*");

        AddImage("RightTurnoutUnknown", "Track_Turnout_Right", "**XD**S*"); // Support all 8 rotations
        AddImage("RightTurnoutStraight", "Track_Turnout_Right_Straight", "**XD**S*");
        AddImage("RightTurnoutDiverging", "Track_Turnout_Right_Diverging", "**XD**S*");

        Console.WriteLine($"Done Building Available Images with count={Images.Count}");
    }

    public static SvgImage GetImage(string name, int direction = 0) {
        var reference = GetImageReference(name, direction);
        if (reference is null) throw new SvgImageException($"***** Image '{name}' not found");
        return new SvgImage(filename: reference.Filename,
                            rotation: reference.Rotation,
                            connections: reference.Connections
        );
    }

    private static SvgReference GetImageReference(string name, int rotation) {
        if (!Images.TryGetValue(name.ToLower(), out var imageRoot)) throw new SvgImageException($"Image {name} not found");
        return imageRoot.Values.Count switch {
            0 => throw new SvgImageException($"Image {name} has no directional images."),
            1 => imageRoot[0],
            _ => imageRoot.TryGetValue(rotation, out var reference) ? reference : imageRoot[0]
        };
    }

    /// <summary>
    /// Add a non-repeating Image (Button, Image, Circle) etc. These are more used as Icons. 
    /// </summary>
    private static void AddImage(string name, string filename) {
        if (!Images.ContainsKey(name.ToLower())) Images.Add(name.ToLower(), new Dictionary<int, SvgReference>());
        var imageRoot = Images[name.ToLower()];
        if (!imageRoot.ContainsKey(0)) {
            imageRoot.Add(0, new SvgReference(GetFullPathImage(filename), 0, new SvgConnections("********", 0)));
        }
    }

    private static void AddImage(string name, string filename, int points, int start = 0) => AddImage(name, filename, "********", points, start);

    /// <summary>
    /// Add a repeating image to the list. Ideally we build up so that every Image has 8 points that we can get
    /// a display image for. 
    /// </summary>
    /// <param name="name">Name for the root of this Image</param>
    /// <param name="filename">Filename for the rotated image</param>
    /// <param name="connections">What are the base connection points (from 0)</param>
    /// <param name="points">How many points to add? 4 or 8 normally</param>
    /// <param name="start">Where do we start adding? Normally 0 or 45</param>
    private static void AddImage(string name, string filename, string connections, int points = 8, int start = 0) {
        if (!Images.ContainsKey(name.ToLower())) Images.Add(name.ToLower(), new Dictionary<int, SvgReference>());
        var imageRoot = Images[name.ToLower()];
        for (var direction = 0; direction < 8; direction += (8 / points)) {
            var key = start + (direction * 45);
            var rotation = (direction * 45);
            if (!imageRoot.ContainsKey(key)) {
                imageRoot.Add(key, new SvgReference(GetFullPathImage(filename), rotation, new SvgConnections(connections, rotation)));
            }
        }
    }

    private static string GetFullPathImage(string filename) {
        if (!filename.EndsWith(".svg")) filename += ".svg";
        return AvailableSymbols.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x.EndsWith(filename, StringComparison.InvariantCultureIgnoreCase)) ?? throw new FileNotFoundException($"File not found: {filename}");
    }

    private static List<string> BuildAvailableSymbols() {
        var availableSymbols = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        availableSymbols.AddRange(resourceNames.Where(name => name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)).ToList());
        return availableSymbols ?? throw new ApplicationException("No SVG Symbols for Tracks found in this assembly.");
    }
}