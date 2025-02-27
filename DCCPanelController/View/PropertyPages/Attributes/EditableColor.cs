using System.Diagnostics;
using DCCPanelController.View.Components;
using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableColorAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new ColorPickerButton { WidthRequest = 100, HeightRequest = 30, AllowsNoColor = true};
            cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Color: {e.Message}");
            return null;
        }
    }
}