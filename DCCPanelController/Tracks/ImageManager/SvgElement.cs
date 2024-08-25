namespace DCCPanelController.Tracks.ImageManager;

public static class SvgElement {

    /// <summary>
    ///  Simple Map to convert Enum to String and visa-versa. Could just use the enum conversion but
    ///  might want flexibility as the text words are the element names (id='name') inside the SVG Image. 
    /// </summary>
    private static Dictionary<SvgElementEnum, string> ElementMap = new Dictionary<SvgElementEnum, string>() {
        { SvgElementEnum.Track, "Track" },
        { SvgElementEnum.TrackDiverging, "TrackDiverging" },
        { SvgElementEnum.Dashline, "Dashline" },
        { SvgElementEnum.Border, "Border" },
        { SvgElementEnum.Occupied, "Occupied" },
        { SvgElementEnum.Terminator, "Terminator" },
        { SvgElementEnum.Continuation, "Continuation" },
        { SvgElementEnum.Button, "Button" },
        { SvgElementEnum.ButtonOutline, "ButtonOutline" },
        { SvgElementEnum.Text, "Text" },
    };

    public static List<string> ElementNames => ElementMap.Values.ToList();
    
    public static string ToString(SvgElementEnum svgElementEnum) {
        return ElementMap[svgElementEnum] ?? "";
    }

    public static SvgElementEnum ToEnum(string elementName) {
        return ElementMap.FirstOrDefault(x => x.Value.Equals(elementName, StringComparison.OrdinalIgnoreCase)).Key; 
    }

}

/// <summary>
/// These are the actual elements in the SVG Files. 
/// </summary>
public enum SvgElementEnum {
    Track,              // The main part of the Track
    TrackDiverging,     // On a point where there is a diverging track (switched)
    Dashline,           // The central line which can be solid or dashed
    Border,             // The outline around the track - used for Mainline indication or Branchline
    Terminator,         // The end-of-Track terminator
    Occupied,           // The outline around the track border used to indicate Occupied
    Continuation,       // The Arrow or Line indicating that the track continues on another page 
    Button,             // A Button
    ButtonOutline,      // The outline of a button
    Text,               // Text
    Unknown
}
