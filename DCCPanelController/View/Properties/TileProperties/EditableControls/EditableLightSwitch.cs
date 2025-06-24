using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableLightSwitchAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var lights = profile.Lights.ToList();
            _ = dropDownWidth;
            _ = dropDownHeight;

            var cell = new PopupSelector() {
                SelectorType = PopupSelectorTypeEnum.Automatic,
                InnerMargin = new Thickness(10, 0, 0, 0),
                WidthRequest = width,
                DropDownWidth = dropDownWidth,
                DropDownHeight = dropDownHeight,
                DropdownBorderWidth = 0,
                TextSize = 12,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                DisplayFormat = "{Name} ({Id})",
                IsEnabled = lights.Count > 0,
                Placeholder = lights.Count > 0 ? "Select a Light" : "No lights available",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            cell.ItemsSource = lights;
            cell.SetBinding(PopupSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PopupSelector.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Light: {e.Message}");
            return null;
        }
    }
}