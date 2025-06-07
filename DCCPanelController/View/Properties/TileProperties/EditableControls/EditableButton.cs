using System.Reflection;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableButton(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        return new EditableUndefined("Undefined").CreateView(owner, info);

        // try {
        //     var cell = new Switch { BindingContext = value.Owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
        //     cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        //     return cell;
        // } catch (Exception e) {
        //     Console.WriteLine($"Unable to create a Bool switch {e.Message}");
        //     return null;
        // }
    }
}