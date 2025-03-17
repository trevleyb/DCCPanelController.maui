using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel.Helpers;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.View.DynamicProperties;

public class EditableBool : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new Switch { BindingContext = owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
            cell.VerticalOptions = LayoutOptions.Center;            
            cell.SetBinding(Switch.IsToggledProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Bool switch {e.Message}");
            return null;
        }
    }
}