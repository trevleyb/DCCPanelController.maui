using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableRouteAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var routes = profile.Routes.ToList();
            
            var picker = new Picker {
                ItemsSource = routes,
                IsEnabled = routes.Count > 0,
                Title = routes.Count > 0 ? "Select a Route" : "No available routes",
                HorizontalOptions = LayoutOptions.Start
            };
            picker.SetBinding(Picker.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            picker.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Picker.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(picker);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Route: {e.Message}");
            return null;
        }
    }
}