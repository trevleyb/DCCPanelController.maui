using System.Diagnostics;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTrackImageAttribute : EditableAttribute, IEditableAttribute {
    public TrackStyleImageEnum[] TrackTypes { get; set; } = [];

    public IView? CreateView(EditableDetails value) {
        try {
            var attr = value.EditableAttribute as EditableTrackImageAttribute;
            return CreateRadioGroupForEnums("Track Style", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a TrackImageType: {e.Message}");
            return null;
        }
    }
}