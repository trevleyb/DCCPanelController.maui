using System.Reflection;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableUndefined(string label = "undefined", string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object? owner, PropertyInfo? info) {
        try {
            return new Label {
                Text = Label,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Bool switch {e.Message}");
            return null;
        }
    }
}