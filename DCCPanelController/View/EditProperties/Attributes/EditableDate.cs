using System.Diagnostics;
using DCCPanelController.View.EditProperties.Base;

namespace DCCPanelController.View.EditProperties.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableDateAttribute : EditableAttribute, IEditableAttribute {
    public DateOnly MinValue { get; set; } = DateOnly.MinValue; // used for Int (Minimum Value)
    public DateOnly MaxValue { get; set; } = DateOnly.MaxValue; // used for Int (Maximum Value)

    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new DatePicker { BindingContext = value.Owner, Format = "D" };
            cell.SetBinding(DatePicker.DateProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Date: {e.Message}");
            return null;
        }
    }
}