using System.Diagnostics;
using DCCPanelController.View.Components;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableColorAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new ColorPickerButton { WidthRequest = 100, HeightRequest = 30 };
            cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Color: {e.Message}");
            return null;
        }
    }
}