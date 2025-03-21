using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableDate(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public DateOnly MinValue { get; set; } = DateOnly.MinValue; // used for Int (Minimum Value)
    public DateOnly MaxValue { get; set; } = DateOnly.MaxValue; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new DatePicker { BindingContext = owner, Format = "D" };
            cell.VerticalOptions = LayoutOptions.Center;
            cell.SetBinding(DatePicker.DateProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return CreateGroupCell(cell, owner, info);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Date: {e.Message}");
            return null;
        }
    }
}