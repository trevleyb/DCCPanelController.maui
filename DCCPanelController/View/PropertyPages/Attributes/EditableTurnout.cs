using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableTurnoutAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        // TODO: Add support so we can set a Turnout State
        return null;
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