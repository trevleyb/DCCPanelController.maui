using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableInfo : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new Label { BindingContext = owner, Text = attribute.Description };
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Info type: {e.Message}");
            return null;
        }
    }
}