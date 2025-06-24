using System.Reflection;
using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableRouteAttribute(string label, string description = "", int order = 0, string? group = null, int width = 250, int dropDownWidth = 300, int dropDownHeight = 200)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var profile = MauiProgram.ServiceHelper.GetService<Profile>();
            var routes = profile.Routes.ToList();
            
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
                IsEnabled = routes.Count > 0,
                Placeholder = routes.Count > 0 ? "Select a Route" : "No available routes",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };
            cell.ItemsSource = routes;
            cell.SetBinding(PopupSelector.SelectedValueProperty, new Binding(info.Name) { Source = owner, Mode = BindingMode.TwoWay });
            cell.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(PopupSelector.SelectedValue)) SetModified(true);
            };
            return CreateGroupCell(cell);
        } catch (Exception e) {
            Console.WriteLine($"Unable to create a Route: {e.Message}");
            return null;
        }
    }
}