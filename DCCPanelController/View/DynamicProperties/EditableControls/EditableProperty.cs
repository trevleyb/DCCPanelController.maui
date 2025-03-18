using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public abstract class EditableProperty {
    
    protected IView CreateGroupCell(IView view, object owner, PropertyInfo info, EditableAttribute attribute) {
        
        var grid = new Grid();
        grid.HeightRequest = 43;
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 150 });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition() { Height = 3 });
        grid.BackgroundColor = Colors.White;
        
        // If there is a Label, then add the label to the first column. 
        // -----------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(attribute.Label)) {
            var label = new Label {
                Text = attribute.Label,
                TextColor = Colors.DimGray,
                FontSize = 15,
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 5, 5),
            };
            grid.Children.Add(label);
            grid.SetColumn(label, 0);
            grid.SetRow(label, 0);
        }

        // Add the view to the 2nd column on the 1st row
        // -----------------------------------------------------------------------------
        grid.Children.Add(view);
        grid.SetColumn(view, 1);
        grid.SetRow(view, 0);
        
        // Add a divider line into the 2nd row and span across both columns
        // -----------------------------------------------------------------------------
        var boxView = new BoxView {
            BackgroundColor = Colors.Gainsboro,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(5, 5, 5, 5)
        };
        grid.Children.Add(boxView);
        grid.SetColumn(boxView, 0);
        grid.SetRow(boxView, 1);
        grid.SetColumnSpan(boxView, 2);

        return grid;
    }
}