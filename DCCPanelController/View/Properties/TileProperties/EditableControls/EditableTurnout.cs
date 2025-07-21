using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Components;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableTurnoutAttribute(string label, string description = "", int order = 0, string? group = null, int width = 300, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var turnouts =((Entity)owner).Parent?.Turnouts.ToList() ?? new List<Turnout>();

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
                IsEnabled = turnouts.Count > 0,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                DisplayFormat = "{Name} ({Id})",
                Placeholder = turnouts.Count > 0 ? "Select a Turnout" : "No available turnouts",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            cell.ItemsSource = turnouts;
            cell.SetBinding(PopupSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PopupSelector.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Turnout: {Message}",e.Message);
            return null;
        }
    }
}