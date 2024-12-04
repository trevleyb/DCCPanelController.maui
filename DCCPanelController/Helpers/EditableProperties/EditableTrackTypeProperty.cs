using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.Attributes;

public class EditableTrackTypePropertyAttribute : EditableProperty {
    public TrackStyleType[] TrackTypes { get; set; } = [];
}
