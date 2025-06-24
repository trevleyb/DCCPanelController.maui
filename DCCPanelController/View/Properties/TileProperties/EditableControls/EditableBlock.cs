using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableBlockAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var blocks = profile.Blocks.ToList();

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
                IsEnabled = blocks.Count > 0,
                Placeholder = blocks.Count > 0 ? "Select an Occupancy Block" : "No occupancy blocks available",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            cell.ItemsSource = blocks;
            cell.SetBinding(PopupSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PopupSelector.SelectedItem)) {
                    if (sender is PopupSelector dropDown) {
                        SetModified(dropDown.SelectedItem != Value);
                    }
                }
            };

            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Block: {e.Message}");
            return null;
        }
    }
}