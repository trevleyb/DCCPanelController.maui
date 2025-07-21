using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableDate(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public DateOnly MinValue { get; set; } = DateOnly.MinValue; // used for Int (Minimum Value)
    public DateOnly MaxValue { get; set; } = DateOnly.MaxValue; // used for Int (Maximum Value)

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new DatePicker { BindingContext = owner, Format = "D" };
            cell.VerticalOptions = LayoutOptions.Center;
            cell.SetBinding(DatePicker.DateProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(DatePicker.Date)) {
                    if (sender is DatePicker item) {
                        SetModified(item.Date != (DateTime)(Value ?? new DateTime(1,1,1970)));
                    }
                }
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Date: {Message}",e.Message);
            return null;
        }
    }
}