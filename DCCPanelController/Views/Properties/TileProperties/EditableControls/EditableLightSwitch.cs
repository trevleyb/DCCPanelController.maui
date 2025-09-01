using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Views.Components;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Views.Properties.TileProperties.EditableControls;

public class EditableLightSwitchAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var lights = ((Entity)owner).Parent?.Lights.ToList() ?? new List<Light>();

            _ = dropDownWidth;
            _ = dropDownHeight;

            var cell = new PickerSelector {
                WidthRequest = width,
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
            cell.SetBinding(PickerSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PickerSelector.SelectedItem)) SetModified(true);
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Light: {Message}", e.Message);
            return null;
        }
    }
}