using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.EditableProperties;

public class EditableTrackTypePropertyAttribute : EditableProperty {
    public TrackStyleType[] TrackTypes { get; set; } = [];
}
