using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.Attributes;

public class EditableTrackImagePropertyAttribute : EditableProperty {
    public TrackStyleImage[] TrackTypes { get; set; } = [];
}