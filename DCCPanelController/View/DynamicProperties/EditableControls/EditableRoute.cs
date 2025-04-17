using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties;

public class EditableRouteAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var routes  = profile.Routes.ToList();
            var cell = new PopUpListBox() {
                ItemsSource = routes,
                IsEnabled = routes.Count > 0,
                Placeholder = "Select a Route",
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