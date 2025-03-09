namespace DCCPanelController.Models.ViewModel.ImageManager;

public static class SvgElementTypes {
    public static (string Element, string Attribute) GetElement(SvgElementType elementType) => elementType switch {
        SvgElementType.Track          => ("Track", "fill"),
        SvgElementType.TrackDiverging => ("TrackDiverging","fill"),
        SvgElementType.Dashline      => ("Dashline","fill"),
        SvgElementType.Border        => ("Border","fill"),
        SvgElementType.Occupied      => ("Occupied","fill"),
        SvgElementType.Terminator    => ("Terminator","fill"),
        SvgElementType.Continuation  => ("Continuation","fill"),
        SvgElementType.Button        => ("Button","fill"),
        SvgElementType.ButtonOutline => ("ButtonOutline","fill"),
        SvgElementType.Text          => ("Text","fill"),
        _                            => ("","")
    };
}

/// <summary>
///     These are the actual elements in the SVG Files.
/// </summary>
public enum SvgElementType {
    Track,          // The main part of the Track
    TrackDiverging, // On a point where there is a diverging track (switched)
    Dashline,       // The central line which can be solid or dashed
    Border,         // The outline around the track - used for Mainline indication or Branchline
    Terminator,     // The end-of-Track terminator
    Occupied,       // The outline around the track border used to indicate Occupied
    Continuation,   // The Arrow or Line indicating that the track continues on another page 
    Button,         // A Button
    ButtonOutline,  // The outline of a button
    Text,           // Text
    Unknown
}