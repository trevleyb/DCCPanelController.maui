using System.Reflection;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableString(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = Label,
                Keyboard = Keyboard.Text,
                WidthRequest = 300,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };

            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }
}