using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Views.Components;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Views.Properties.TileProperties.EditableControls;

public class EditableColor(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new ColorPickerButton {
                WidthRequest = 150, HeightRequest = 30, AllowsNoColor = true,
                VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start
            };
            cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            if (owner is ITrackEntity && info.Name == nameof(TrackEntity.TrackBorderColor)) {
                cell.SetBinding(VisualElement.IsEnabledProperty, new Binding(nameof(TrackEntity.IsMainLine)) { Source = owner, Mode = BindingMode.TwoWay });
            }
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(ColorPickerButton.SelectedColor)) {
                    if (sender is ColorPickerButton picker) {
                        if (picker.SelectedColor == null && Value == null) {
                            SetModified(false);
                        } else if (picker.SelectedColor == null && Value != null) {
                            SetModified(true);
                        } else {
                            SetModified(!picker.SelectedColor?.Equals(Value) ?? false);
                        }
                    }
                }
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Color: {Message}",e.Message);
            return null;
        }
    }
}