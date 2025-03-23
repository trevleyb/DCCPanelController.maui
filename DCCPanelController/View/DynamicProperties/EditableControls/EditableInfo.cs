using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableInfo(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new Label { BindingContext = owner, Text = Description };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Info type: {e.Message}");
            return null;
        }
    }
}