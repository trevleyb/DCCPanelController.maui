using System.Diagnostics;
using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties;

public class EditableColor(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null) {
        try {
            var cell = new ColorPickerButton {
                WidthRequest = 100, HeightRequest = 30, AllowsNoColor = true, 
                VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start,
            };
            cell.SetBinding(ColorPickerButton.SelectedColorProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            if (owner is ITrackEntity && info.Name == nameof(TrackEntity.TrackBorderColor)) {
                cell.SetBinding(VisualElement.IsEnabledProperty, new Binding(nameof(TrackEntity.IsMainLine)) { Source = owner, Mode = BindingMode.TwoWay });
            }
            cell.PropertyChanged += (_, _) => propertyModified?.Invoke(info.Name);
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Debug.WriteLine($"Unable to create a Color: {e.Message}");
            return null;
        }
    }
}