using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public class EditableTurnoutAttribute(string label, string description = "", int order = 0, string? group = null, int width = 150)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var turnouts = profile.Turnouts.Select(t => t.Id).ToList();
            var cell = new PopUpListBox {
                WidthRequest = width,
                ItemsSource = turnouts,
                IsEnabled = turnouts.Count > 0,
                Placeholder = "Select a Turnout",
                HorizontalOptions = LayoutOptions.Start
            };
            cell.SetBinding(DropDownBoxBase.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Route: {e.Message}");
            return null;
        }
    }
}