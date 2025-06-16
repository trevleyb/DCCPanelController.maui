using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableBlockAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var blocks = profile.Blocks.Select(t => t.Id).ToList();

            var picker = new Picker {
                WidthRequest = width,
                ItemsSource = blocks,
                IsEnabled = blocks.Count > 0,
                Title = blocks.Count > 0 ? "Select an Occupancy Block" : "No occupancy blocks available",
                HorizontalOptions = LayoutOptions.Start
            };
            picker.SetBinding(Picker.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            picker.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Picker.SelectedItem)) {
                    if (sender is Picker dropDown) {
                        SetModified(dropDown.SelectedItem != Value);
                    }
                }
            };
            return CreateGroupCell(picker);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Block: {e.Message}");
            return null;
        }
    }
}