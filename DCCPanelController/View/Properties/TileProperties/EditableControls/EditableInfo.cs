using System.Reflection;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableInfo(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new Label { BindingContext = owner, Text = Description };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Info type: {e.Message}");
            return null;
        }
    }
}