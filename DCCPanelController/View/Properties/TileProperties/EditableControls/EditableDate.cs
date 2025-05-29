using System.Reflection;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableDate(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public DateOnly MinValue { get; set; } = DateOnly.MinValue; // used for Int (Minimum Value)
    public DateOnly MaxValue { get; set; } = DateOnly.MaxValue; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var cell = new DatePicker { BindingContext = owner, Format = "D" };
            cell.VerticalOptions = LayoutOptions.Center;
            cell.SetBinding(DatePicker.DateProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Date: {e.Message}");
            return null;
        }
    }
}