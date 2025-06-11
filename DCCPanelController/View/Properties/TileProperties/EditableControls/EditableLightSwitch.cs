using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableLightSwitchAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var lights = profile.Lights.ToList();
            var cell = new PopUpListBox {
                ItemsSource = lights,
                IsEnabled = lights.Count > 0,
                Placeholder = lights.Count > 0 ? "Select a Light" : "No available lights",
                HorizontalOptions = LayoutOptions.Start
            };
            cell.SetBinding(DropDownBoxBase.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(DropDownBoxBase.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Light: {e.Message}");
            return null;
        }
    }
}