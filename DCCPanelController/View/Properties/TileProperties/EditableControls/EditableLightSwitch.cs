using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableLightSwitchAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var lights = profile.Lights.Select(t => t.Id).ToList();

            _ = dropDownWidth;
            _ = dropDownHeight;
            
#if IOS || ANDROID
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
#else
            var cell = new DropDownListBox {
                WidthRequest = width,
                DropDownWidth = dropDownWidth,
                DropDownHeight = dropDownHeight,
                TextSize = 9,
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
#endif
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Light: {e.Message}");
            return null;
        }
    }
}