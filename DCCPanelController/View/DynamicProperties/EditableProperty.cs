using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public abstract class EditableProperty {
    
    protected IView CreateGroupCell(IView view, object owner, PropertyInfo info, EditableAttribute attribute) {
        var groupCell = new HorizontalStackLayout {
            Margin = new Thickness(0, 5, 0, 5),
        };

        if (!string.IsNullOrWhiteSpace(attribute.Label)) {
            var label = new Label {
                Text = attribute.Label,
                TextColor = Colors.DimGray,
                FontSize = 15,
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 5, 5),
                WidthRequest = 200
            };
            groupCell.Children.Add(label);
        }
        groupCell.Children.Add(view);
        return groupCell;
    }
}