using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Maui.Core.Extensions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.View.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Picker;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableBlockAttribute(string label, string description = "", int order = 0, string? group = null, int width = 200, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {

    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            
            var blocks = ((Entity)owner).Parent?.Blocks.ToList() ?? new List<Block>();
            
            _ = dropDownWidth;
            _ = dropDownHeight;
            
            var cell = new PickerSelector() {
                WidthRequest = width,
                ShowClearFieldImage = blocks.Count > 0,
                TextSize = 12,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                DisplayFormat = "{Name} ({Id})",
                IsEnabled = blocks.Count > 0,
                Placeholder = blocks.Count > 0 ? "Select an Occupancy Block" : "No occupancy blocks available",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            
            //cell.ItemsSource = blocks;
            cell.SetBinding(PickerSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PickerSelector.SelectedItem)) {
                    if (sender is PickerSelector dropDown) {
                        SetModified(dropDown.SelectedItem != Value);
                    }
                }
            };
            
            return CreateGroupCell(cell);
            
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Block: {Message}", e.Message);
            return null;
        }
    }

}