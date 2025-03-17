using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableButton : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new EditableUndefined().CreateView(owner, info, attribute);
        
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