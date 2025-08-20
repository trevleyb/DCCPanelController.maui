using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Components;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableTurnoutAttribute(string label, string description = "", int order = 0, string? group = null, int width = 200, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var initialValue = info.GetValue(owner);
            var turnouts =((Entity)owner).Parent?.Turnouts.ToList() ?? new List<Turnout>();

            _ = dropDownWidth;
            _ = dropDownHeight;
            
            var cell = new PickerSelector() {
                ShowClearFieldImage = turnouts.Count > 0, 
                WidthRequest = width,
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
            cell.SetBinding(PickerSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PickerSelector.SelectedItem)) {
                    var currentValue = info.GetValue(owner);
                    if (!object.Equals(currentValue, initialValue)) {
                        SetModified(true);
                    }
                }
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Turnout: {Message}",e.Message);
            return null;
        }
    }
}