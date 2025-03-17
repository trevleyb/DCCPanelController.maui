using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Helpers;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.View.DynamicProperties;

public class EditableUndefined : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            return new Label {
                Text = "Undefined Editable Property",
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Bool switch {e.Message}");
            return null;
        }
    }
}