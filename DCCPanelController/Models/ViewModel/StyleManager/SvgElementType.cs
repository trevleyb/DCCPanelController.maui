namespace DCCPanelController.Models.ViewModel.StyleManager;

public enum SvgElementType {
    Track,          // The main part of the Track
    TrackDiverging, // On a point where there is a diverging track (switched)
    Dashline,       // The central line which can be solid or dashed
    Border,         // The outline around the track - used for Mainline indication or Branchline
    Terminator,     // The end-of-Track terminator
    Continuation,   // The Arrow or Line indicating that the track continues on another page 
    Button,         // A Button
    ButtonOutline,  // The outline of a button
    Text,           // Text
    Highlight,
    Indicator,
    Bridge,
    Platform,
    Tunnel,
    Unknown
}