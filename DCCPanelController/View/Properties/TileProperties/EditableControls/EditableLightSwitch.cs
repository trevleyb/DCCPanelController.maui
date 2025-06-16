using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableLightSwitchAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var lights = profile.Lights.Select(t => t.Id).ToList();

            var picker = new Picker {
                WidthRequest = width,
                ItemsSource = lights,
                IsEnabled = lights.Count > 0,
                Title = lights.Count > 0 ? "Select a Light" : "No available lights",
                HorizontalOptions = LayoutOptions.Start
            };
            picker.SetBinding(Picker.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            picker.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Picker.SelectedItem)) SetModified(true);
            };

            return CreateGroupCell(picker);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Light: {e.Message}");
            return null;
        }
    }
}