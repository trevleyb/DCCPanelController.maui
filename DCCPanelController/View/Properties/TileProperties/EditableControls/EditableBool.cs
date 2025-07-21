using System.Reflection;
using DCCPanelController.Helpers;
using Microsoft.Extensions.Logging;
using Switch = Microsoft.Maui.Controls.Switch;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableBool(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new Switch { BindingContext = owner, OnColor = StyleColor.Get("Primary"), ThumbColor = Colors.White };
            cell.VerticalOptions = LayoutOptions.Center;
            cell.SetBinding(Switch.IsToggledProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (sender is Switch sw){
                    if (args.PropertyName == nameof(Switch.IsToggled)) SetModified(sw.IsToggled == (bool)(Value ?? false));
                }
            };
           return CreateGroupCell(cell);
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Bool switch {Message}",e.Message);
            return null;
        }
    }
}