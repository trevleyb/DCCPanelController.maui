using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTrackImagePropertyAttribute : EditableProperty {
    public TrackStyleImage[] TrackTypes { get; set; } = [];
}