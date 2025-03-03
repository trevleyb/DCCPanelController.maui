using System.Diagnostics;
using DCCPanelController.View.PropertyPages.Base;

namespace DCCPanelController.View.PropertyPages.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditableStringAttribute : EditableAttribute, IEditableAttribute {
    public IView? CreateView(EditableDetails value) {
        try {
            var cell = new Entry {
                Placeholder = Description,
                Keyboard = Keyboard.Text,
                WidthRequest = 300,
                VerticalOptions = LayoutOptions.Center,
                BindingContext = value.Owner
            };

            cell.SetBinding(Entry.TextProperty, new Binding(value.Info.Name) { Source = value.Owner, Mode = BindingMode.TwoWay });
            return cell;
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a String:  {e.Message}");
            return null;
        }
    }
}