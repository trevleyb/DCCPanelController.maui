using System.Reflection;
using DCCClients.WiThrottle.ServiceHelper;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.DynamicProperties;

public class EditableRouteAttribute(string label, string description = "", int order = 0, string? group = null)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var cell = new RoutePickerButton();
            cell.SetBinding(RoutePickerButton.SelectedRouteProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            cell.AvailableRoutes = profile.Routes.ToList();
            return cell;
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Route: {e.Message}");
            return null;
        }
    }
}