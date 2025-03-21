using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableTurnoutAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {        // TODO: Add support so we can set a Turnout State
        return new EditableUndefined("undefined").CreateView(owner, info);

        // try {
        //     //var cell = new Switch { BindingContext = value.Owner };
        //     //cell.SetBinding(Switch.IsToggledProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
        //     //return cell;    
        //     return null;
        // } catch (Exception e) {
        //     Debug.WriteLine($"Unable to create a Turnout: {e.Message}");
        // return null;
        //}
    }
}