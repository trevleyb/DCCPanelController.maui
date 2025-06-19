using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgImages {
    public static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

    private static readonly Lock LockObject = new();
    private static readonly List<string> AvailableSymbols;
    private static readonly Dictionary<string, Dictionary<int, SvgReference>> Images = new();

    static SvgImages() {
        AvailableSymbols = BuildAvailableSymbols();
        using (new CodeTimer("Building Image Cache", false)) {
            BuildAvailableImages();
        }
    }

    private static void BuildAvailableImages() {
        AddImage("Unknown", "Track_Unknown");

        AddImage("Text", "Track_Text");
        AddImage("Label", "Track_Label");
        AddImage("Image", "Track_Image");
        AddImage("Compass", "Track_Compass");
        AddImage("Points", "Track_Points");
        AddImage("Circle", "draw_circle");
        AddImage("Rectangle", "draw_rectangle");
        AddImage("Line", "draw_line");

        AddImage("ButtonLarge", "Track_Large_Button");
        AddImage("Button", "Track_Button", "Track_Button_Corner");

        AddImage("Switch", "switch_on");
        AddImage("SwitchOn", "switch_on");
        AddImage("SwitchOff", "switch_off");
        AddImage("Light", "light");

        AddImage("Route", "Track_Route");
        AddImage("RouteLarge", "Track_Route_Large");

        AddImage("Platform", "Track_Platform");
        AddImage("Straight", "Track_Straight", "Track_Angle");
        AddImage("Tunnel", "Track_Straight_Tunnel", "Track_Angle_Tunnel");
        AddImage("Cross", "Track_Straight_Cross", "Track_Angle_Cross");
        AddImage("Corner", "Track_Corner_Left", "Track_Corner_Right");
        AddImage("Terminator", "Track_Straight_Terminator", "Track_Angle_Terminator");

        AddImage("StraightContinuationArrow", "Track_Straight_Continuation_Arrow", "Track_Angle_Continuation_Arrow");
        AddImage("StraightContinuationLines", "Track_Straight_Continuation_Lines","Track_Angle_Continuation_Lines");

        AddImage("CornerContinuationArrow", "Track_Corner_Left_Continuation_Arrow","Track_Corner_Right_Continuation_Arrow");
        AddImage("CornerContinuationLines", "Track_Corner_Left_Continuation_Lines", "Track_Corner_Right_Continuation_Lines");

        AddImage("LeftTurnoutUnknown", "Track_Turnout_Left");
        AddImage("LeftTurnoutStraight", "Track_Turnout_Left_Straight");
        AddImage("LeftTurnoutDiverging", "Track_Turnout_Left_Diverging");

        AddImage("RightTurnoutUnknown", "Track_Turnout_Right");
        AddImage("RightTurnoutStraight", "Track_Turnout_Right_Straight");
        AddImage("RightTurnoutDiverging", "Track_Turnout_Right_Diverging");
    }

    public static int CompassPoints(int direction) {
        if (direction is >= 0 and < 8) return direction;
        return direction switch {
            < 45   => 0,    // North
            < 90   => 1,    // North-East
            < 135  => 2,    // East
            < 180  => 3,    // South East
            < 225  => 4,    // South
            < 270  => 5,    // South West
            < 315  => 6,    // West
            >= 315 => 7,    // North West
        };
    }

    public static SvgImage GetImage(string name, int direction = 0) {
        var reference = GetImageReference(name, direction);
        if (reference is null) throw new SvgImageException($"***** Image '{name}' not found");
        return new SvgImage(reference.Filename, reference.Rotation);
    }

    private static SvgReference GetImageReference(string name, int rotation) {
        try {
            if (!Images.TryGetValue(name.ToLower(), out var imageRoot)) throw new SvgImageException($"Image {name} not found");
            return imageRoot.Values.Count switch {
                0 => throw new SvgImageException($"Image {name} has no directional images."),
                1 => imageRoot[0],
                _ => imageRoot.TryGetValue(rotation, out var reference) ? reference : imageRoot[0]
            };
        } catch (Exception ex) {
            Console.WriteLine($"Error getting image: {name} @ {rotation}");
            throw new SvgImageException($"Image {name} has no directional images.");
        }
    } 

    /// <summary>
    ///     Add a non-repeating Image (Button, Image, Circle) etc. These are more used as Icons.
    /// </summary>
    private static void AddImage(string name, string filename) => AddImage(name, filename, filename);

    private static void AddImage(string name, string filenameStraight, string filenameDiagonal) {
        if (!Images.ContainsKey(name.ToLower())) Images.Add(name.ToLower(), new Dictionary<int, SvgReference>());
        var imageRoot = Images[name.ToLower()];

        // The default starting direction for any tile or entity is East-West. So assume
        // that we start with a position of (2) East as a rotation angle of 0. 
        // -----------------------------------------------------------------------------
        for (var direction = 0; direction < 8; direction ++) {
            
            // Key is the rotation that we need to search for this image.
            // This is the rotation value associated with the track Entity, such as 45
            // This should then return the correct image
            // ------------------------------------------------------------------------
            var key = (direction * 45) % 360;
            
            // Rotation is how much this image should be rotated when displayed. 
            // While we may want the entity at a 45-degree rotation, if it is a angle
            // image, then the image should actually be at 45 already, so the rotation is 0. 
            // ------------------------------------------------------------------------
            var rotation = direction % 2 == 0 ? key : key - 45;
            var imageToUse = direction % 2 == 0 ? filenameStraight : filenameDiagonal;
            
            if (!imageRoot.ContainsKey(key)) {
                var foundImage = GetFullPathImage(imageToUse);
                if (string.IsNullOrEmpty(foundImage)) {
                    Console.WriteLine($"Image {foundImage} not found");
                    return; // Not found, so don't add it to the list.'
                }
                imageRoot.Add(key, new SvgReference(foundImage, rotation));
            }
        }
    }

    
    private static void AddImage(string name, string filename, int points, int start) {
        if (!Images.ContainsKey(name.ToLower())) Images.Add(name.ToLower(), new Dictionary<int, SvgReference>());
        var imageRoot = Images[name.ToLower()];
        for (var direction = 0; direction < 8; direction += 8 / points) {
            var key = start + direction * 45;
            var rotation = direction * 45;
            if (!imageRoot.ContainsKey(key)) {
                var foundImage = GetFullPathImage(filename);
                if (string.IsNullOrEmpty(foundImage)) {
                    Console.WriteLine($"Image {filename} not found");
                    return; // Not found, so don't add it to the list.'
                }
                imageRoot.Add(key, new SvgReference(foundImage, rotation));
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

    private record SvgReference(string Filename, int Rotation);
}