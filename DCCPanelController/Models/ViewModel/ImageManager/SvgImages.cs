using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgImages {
    public static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

    private static readonly Lock LockObject = new();
    private static readonly List<string> AvailableSymbols;
    private static readonly Dictionary<string, (string straight, string diagonal)> Images = new();

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
        AddImage("StraightContinuationLines", "Track_Straight_Continuation_Lines", "Track_Angle_Continuation_Lines");

        AddImage("CornerContinuationArrow", "Track_Corner_Left_Continuation_Arrow", "Track_Corner_Right_Continuation_Arrow");
        AddImage("CornerContinuationLines", "Track_Corner_Left_Continuation_Lines", "Track_Corner_Right_Continuation_Lines");

        AddImage("LeftTurnoutUnknown", "Track_Turnout_Left");
        AddImage("LeftTurnoutStraight", "Track_Turnout_Left_Straight");
        AddImage("LeftTurnoutDiverging", "Track_Turnout_Left_Diverging");

        AddImage("RightTurnoutUnknown", "Track_Turnout_Right");
        AddImage("RightTurnoutStraight", "Track_Turnout_Right_Straight");
        AddImage("RightTurnoutDiverging", "Track_Turnout_Right_Diverging");
    }

    public static SvgImage GetImage(string name, int direction = 0) {
        if (Images.TryGetValue(name.ToLowerInvariant(), out var images)) {
            
            // If we are looking for a straight track piece (----) then return it.
            // -----------------------------------------------------------------------------
            if (direction % 90 == 0) return new SvgImage(images.straight, direction);
            
            // If it is angled, we need, then we need to adjust as all the images have been 
            // built pointing up / but should be \ so need to add 45 to them to adjust
            // -----------------------------------------------------------------------------
            if (direction % 45 == 0) {
                return new SvgImage(images.diagonal, ((direction + 45) % 360) - 90);
            }
            throw new SvgImageException($"***** Image '{name}' invalid direction provided");
        }
        Console.WriteLine($"Could not find the base image of {name}");
        throw new SvgImageException($"***** Image '{name}' not found");
    }

    private static void AddImage(string name, string filename) => AddImage(name, filename, filename);
    private static void AddImage(string name, string filenameStraight, string filenameDiagonal) {
        if (Images.ContainsKey(name.ToLowerInvariant())) return;
        Images.Add(name.ToLowerInvariant(), (GetFullPathImage(filenameStraight), GetFullPathImage(filenameDiagonal)));
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