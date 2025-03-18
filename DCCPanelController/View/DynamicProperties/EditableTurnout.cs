using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableTurnoutAttribute : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {        // TODO: Add support so we can set a Turnout State
        return new EditableUndefined().CreateView(owner, info, attribute);

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
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new ViewCell() { View = CreateView(owner, info, attribute) as Microsoft.Maui.Controls.View };
    }
}