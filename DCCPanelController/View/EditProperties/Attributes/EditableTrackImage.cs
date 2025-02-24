using System.Diagnostics;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AttributesTrackImageAttribute : Base.Attributes, IEditableAttribute {
    public TrackStyleImageEnum[] TrackTypes { get; set; } = [];

    public IView? CreateView(EditableDetails value) {
        try {
            var attr = value.Attribute as AttributesTrackImageAttribute;
            return CreateRadioGroupForEnums("Track Style", attr?.TrackTypes ?? [], value.Owner, value.Info.Name);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a TrackImageType: {e.Message}");
            return null;
        }
    }
}