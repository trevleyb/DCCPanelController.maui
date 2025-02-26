using System.Diagnostics;
using DCCPanelController.Helpers;
using DCCPanelController.View.EditProperties.Base;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableButtonAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        // TODO: Add support for setting a button state
        return null;
        // try {
        //     var cell = new Switch { BindingContext = value.Owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
        //     cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        //     return cell;
        // } catch (Exception e) {
        //     Debug.WriteLine($"Unable to create a Bool switch {e.Message}");
        //     return null;
        // }
    }
}