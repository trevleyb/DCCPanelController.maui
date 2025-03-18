using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableInfo : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new Label { BindingContext = owner, Text = attribute.Description };
            return CreateGroupCell(cell, owner, info, attribute);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Info type: {e.Message}");
            return null;
        }
    }
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
}