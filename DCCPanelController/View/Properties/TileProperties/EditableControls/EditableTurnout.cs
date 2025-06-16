using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableTurnoutAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var turnouts = profile.Turnouts.Select(t => t.Id).ToList();

            _ = dropDownWidth;
            _ = dropDownHeight;
            
#if IOS || ANDROID
                var picker = new Picker {
                    WidthRequest = width,
                    ItemsSource = turnouts,
                    IsEnabled = turnouts.Count > 0,
                    Title = turnouts.Count > 0 ? "Select a Turnout" : "No available turnouts",
                    HorizontalOptions = LayoutOptions.Start
                };
                picker.SetBinding(Picker.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
                picker.PropertyChanged += (sender, args) => {
                    if (args.PropertyName == nameof(Picker.SelectedItem)) SetModified( !(Value?.Equals(picker.SelectedItem) ?? false));
                };

                return CreateGroupCell(picker);
#else
            var cell = new DropDownListBox {
                WidthRequest = width,
                DropDownWidth = dropDownWidth,
                DropDownHeight = dropDownHeight,
                TextSize = 9,
                ItemsSource = turnouts,
                IsEnabled = turnouts.Count > 0,
                Placeholder = turnouts.Count > 0 ? "Select a Turnout" : "No available turnouts",
                HorizontalOptions = LayoutOptions.Start
            };
            cell.SetBinding(DropDownBoxBase.SelectedItemProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(DropDownBoxBase.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(cell);
#endif
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Turnout: {e.Message}");
            return null;
        }
    }
}