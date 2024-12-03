namespace DCCPanelController.Tracks.StyleManager;

// These enums represent the different image types that we might use. 
// They are normally paired Diverging/Stright, Active/InActive, Lines/Arrows
// Normal, Default and Unknown should all have the same effect. 
// ----------------------------------------------------------------------------
public enum TrackStyleImage {
    Normal,
    Default,
    Unknown,
    Straight,
    Diverging,
    Active,
    InActive,
    Lines,
    Arrow,
    Symbol
}

// The TrackStyleType sets up the track as being a Branchline or Mainline track
// This is a set of rules for making the tracks thiner or thicker only
// ----------------------------------------------------------------------------
public enum TrackStyleType {
    Button,
    Mainline,
    Branchline,
}

// Attributes are additional rules we can apply to a track peice to set additional
// attributes such as making the lines dashed or shoing a occupied border. 
// ----------------------------------------------------------------------------
public enum TrackStyleAttribute {
    Hidden,
    Normal,
    Occupied,
}