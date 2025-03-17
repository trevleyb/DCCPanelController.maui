using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties;

public class EditableColor : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new ColorPickerButton { WidthRequest = 100, HeightRequest = 30, AllowsNoColor = true };
            cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Color: {e.Message}");
            return null;
        }
    }
}