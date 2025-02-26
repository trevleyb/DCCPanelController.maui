using System.Diagnostics;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableInfoAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new Label { BindingContext = value.Owner, Text = value.EditableAttribute.Description };
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Info type: {e.Message}");
            return null;
        }
    }
}