using System.Reflection;
using DCCPanelController.Views.Components;

namespace DCCPanelController.Views.Properties.TileProperties.EditableControls;

public class EditableButton(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        
        var initialValue = info.GetValue(owner);
        var cell = new ButtonStateControl() {
            CanToggleState = true,
        };
        cell.SetBinding(ButtonStateControl.StateProperty,new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
        cell.StateChangedCommand = new Command<object>(currentValue => {
            SetModified(!object.Equals(currentValue, initialValue));
        });
        return CreateGroupCell(cell);
    }
}