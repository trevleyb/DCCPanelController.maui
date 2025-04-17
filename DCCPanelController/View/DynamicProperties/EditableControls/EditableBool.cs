using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Helpers;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.View.DynamicProperties;

public class EditableBool(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var cell = new Switch { BindingContext = owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
            cell.VerticalOptions = LayoutOptions.Center;            
            cell.SetBinding(Switch.IsToggledProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Bool switch {e.Message}");
            return null;
        }
    }
}