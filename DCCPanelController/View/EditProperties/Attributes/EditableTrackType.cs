using System.Diagnostics;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTrackTypeAttribute : EditableAttribute, IEditableAttribute {
    public TrackStyleTypeEnum[] TrackTypes { get; set; } = [];

    public IView? CreateView(EditableDetails value) {
        try {
            var attr = value.EditableAttribute as EditableTrackTypeAttribute;
            return CreateRadioGroupForEnums("Track Type", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Track Style: {e.Message}");
            return null;
        }
    }
}