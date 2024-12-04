using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTrackTypePropertyAttribute : EditableProperty {
    public TrackStyleType[] TrackTypes { get; set; } = [];
}