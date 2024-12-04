using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.EditableProperties;

public class EditableTrackImagePropertyAttribute : EditableProperty {
    public TrackStyleImage[] TrackTypes { get; set; } = [];
}