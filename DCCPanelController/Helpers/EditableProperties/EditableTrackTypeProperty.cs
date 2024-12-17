using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTrackTypePropertyAttribute : EditableProperty {
    public TrackStyleTypeEnum[] TrackTypes { get; set; } = [];
}