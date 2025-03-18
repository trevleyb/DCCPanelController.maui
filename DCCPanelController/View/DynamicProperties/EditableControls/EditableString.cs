using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public class EditableString : EditableProperty, IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute) {
        try {
            var cell = new Entry {
                Margin = new Thickness(5, 5, 5, 5),
                Placeholder = attribute.Label,
                Keyboard = Keyboard.Text,
                WidthRequest = 300,
                HeightRequest = 25,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = owner
            };

            cell.SetBinding(Entry.TextProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            return CreateGroupCell(cell, owner, info, attribute);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }
    public Cell? CreateCell(object owner, PropertyInfo info, EditableAttribute attribute) {
        return new  EntryCell { Text = attribute.Label, BindingContext = new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay } };

    }
}