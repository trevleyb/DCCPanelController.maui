using System.Reflection;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.StyleManager;

namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgImages {
    static SvgImages() {
        AvailableSymbols = BuildAvailableSymbols();
        BuildAvailableImages();
    }

    private static readonly Lock LockObject = new();
    private static readonly List<string> AvailableSymbols;
    private static readonly Dictionary<string, Dictionary<SvgDirection, SvgImage>> Images = new();

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
        
        AddImage("Button", "Track_Button");
        AddImage("ButtonCorner", "Track_Button_Corner");

        AddImageFromNorth("Straight", "Track_Straight", "**S***S*");
        AddImageFromNorthEast("Straight", "Track_Angle", "*S***S**");
        
        AddImageFromNorth("Cross", "Track_Straight_Cross", "S*S*S*S*");
        AddImageFromNorthEast("Cross", "Track_Angle_Cross", "*S*S*S*S");
        
        AddImageFromNorth("Corner", "Track_Corner_Left", "*S****S*");
        AddImageFromNorthEast("Corner", "Track_Corner_Right", "***S**S*");
        
        AddImageFromNorth("Terminator", "Track_Straight_Terminator", "**T***S*");
        AddImageFromNorthEast("Terminator", "Track_Angle_Terminator", "*T***S**");

        AddImageFromNorth("StraightContinuationArrow", "Track_Straight_Continuation_Arrow", "**C***S*");
        AddImageFromNorthEast("StraightContinuationArrow", "Track_Angle_Continuation_Arrow", "*C***S**");
        AddImageFromNorth("StraightContinuationLines", "Track_Straight_Continuation_Lines", "**C***S*");
        AddImageFromNorthEast("StraightContinuationLines", "Track_Angle_Continuation_Lines", "*C***S**");

        AddImageFromNorth("CornerContinuationArrow", "Track_Corner_Left_Continuation_Arrow", "*C****S*");
        AddImageFromNorthEast("CornerContinuationArrow", "Track_Corner_Right_Continuation_Arrow", "***C**S*");
        AddImageFromNorth("CornerContinuationLines", "Track_Corner_Left_Continuation_Lines", "*C****S*");
        AddImageFromNorthEast("CornerContinuationLines", "Track_Corner_Right_Continuation_Lines", "***C**S*");

        AddImageFromNorth("LeftTurnoutUnknown", "Track_Turnout_Left", "*DX***S*");
        AddImageFromNorth("LeftTurnoutStraight", "Track_Turnout_Left_Straight", "*DX***S*");
        AddImageFromNorth("LeftTurnoutDiverging", "Track_Turnout_Left_Diverging", "*DX***S*");

        AddImageFromNorth("RightTurnoutUnknown", "Track_Turnout_Right", "**XD**S*");
        AddImageFromNorth("RightTurnoutStraight", "Track_Turnout_Right_Straight", "**XD**S*");
        AddImageFromNorth("RightTurnoutDiverging", "Track_Turnout_Right_Diverging", "**XD**S*");

        Console.WriteLine($"Done Building Available Images with count={Images.Count}");
    }

    /// <summary>
    /// Find the SVGImage object based on the item we want and the direct we need to be in
    /// </summary>
    public static SvgImage GetImage(string name) => GetImage(name, 0);

    public static SvgImage GetImage(string name, int direction) => GetImage(name, (SvgDirection)direction);

    public static SvgImage GetImage(string name, SvgDirection direction) {
        if (!Images.TryGetValue(name.ToLower(), out var imageBase)) throw new SvgImageException($"Image {name} not found");
        if (imageBase.Values.Count == 0) throw new SvgImageException($"Image {name} has no directional images.");
        if (imageBase.Values.Count == 1) {
            Console.WriteLine($"Image {name} has only one directional image. Returning first image.");
            return imageBase.Values.First();
        }
        if (imageBase.TryGetValue(direction, out var image)) {
            Console.WriteLine($"Image {name} has directional image for {direction}.");
            return image;
        }
        Console.WriteLine($"Could not find directional image for {name} in {direction}. Returning first image in list:");
        return imageBase.Values.First();
    }

    /// <summary>
    /// Create an instance of a record that knows about an Image for a given direction and rotation
    /// </summary>
    /// <param name="name">What do we call this image (eg: Straight)</param>
    /// <param name="filename">Where do we find this image</param>
    /// <param name="connections">What are the available connections (specific string format)</param>
    /// <param name="direction">What direction(s) does this image work in</param>
    /// <param name="rotation">What is the rotation that this image should be for the direction</param>
    private static void AddImage(string name, string filename, string connections, SvgDirection direction, int rotation) => AddImage(name, filename, new SvgConnections(connections, rotation), direction, rotation);

    private static void AddImage(string name, string filename, SvgConnections connections, SvgDirection direction, int rotation) {
        var imageRef = GetSvgImageForDirection(name.ToLower(), direction);
        imageRef.Filename = GetFullPathImage(filename);
        imageRef.Rotation = rotation;
        imageRef.Connections = connections;
    }

    /// <summary>
    /// Helper to add simple track image pieces to the collection. The get logic will find only a single image reference
    /// and no matter the rotation or direction will return this item.  
    /// </summary>
    /// <param name="name">The name to add under</param>
    /// <param name="filename">The filename</param>
    private static void AddImage(string name, string filename) {
        AddImage(name.ToLower(), filename, SvgConnections.NoConnections, SvgDirection.North, 0);
    }

    /// <summary>
    /// Special Case always add 4 Points from a starting point, either North or NorthEast 
    /// </summary>
    private static void AddImageFromNorth(string name, string filename, string connections) => AddImagePoints(name, filename, connections, SvgDirection.North, 0);

    private static void AddImageFromNorthEast(string name, string filename, string connections) => AddImagePoints(name, filename, connections, SvgDirection.NorthEast, 45);

    private static void AddImagePoints(string name, string filename, string connections, SvgDirection start, int rotation) {
        for (var i = 0; i < 4; i++) {
            var rotationVal = rotation + (i * 90);
            var directionVal = (SvgDirection)(((int)start + (i * 90)) % 360);
            var connectionsVal = new SvgConnections(connections, rotationVal);
            AddImage(name.ToLower(), filename, connectionsVal, directionVal, (rotationVal - rotation));
        }
    }

    /// <summary>
    /// If we call this with a set of rotations only, assume that these are compass points based on the number.
    /// So if 4 then it is every 4th, if 2, then it is every 2nd,  
    /// </summary>
    private static void AddImage(string name, string filename, ConnectionType[] connections, SvgDirection start, params int[] rotation) {
        var startPos = SvgDirections.GetDirectionIndex(start);
        if (startPos < 0 || startPos >= rotation.Length) throw new ArgumentOutOfRangeException(nameof(start), "Direction must match a valid compass direction.");
        var skip = rotation.Length / SvgConnections.CompassPoints;
        for (var i = 0; i < rotation.Length; i++) {
            var index = startPos + (skip * i);
            if (index >= SvgConnections.CompassPoints) index -= SvgConnections.CompassPoints;
            AddImage(name.ToLower(), filename, connections, (SvgDirection)(index), rotation[i]);
        }
    }

    private static SvgImage GetSvgImageForDirection(string name, SvgDirection direction) {
        if (!Images.ContainsKey(name.ToLower())) Images.Add(name.ToLower(), new Dictionary<SvgDirection, SvgImage>());
        var imageBase = Images[name];
        if (!imageBase.ContainsKey(direction)) imageBase.Add(direction, new SvgImage());
        return imageBase[direction];
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