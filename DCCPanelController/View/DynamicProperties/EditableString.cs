using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableString : IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = attribute.Label,
                Keyboard = Keyboard.Text,
                WidthRequest = 300,
                HeightRequest = 25,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };

            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }
}