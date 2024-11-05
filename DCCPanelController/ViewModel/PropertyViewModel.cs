using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers.Attributes;
using Microsoft.Maui.Controls;

namespace DCCPanelController.ViewModel;

public partial class PropertyViewModel : BaseViewModel {

    [ObservableProperty] string _propertyName;
    
    public PropertyViewModel(string propertName, Grid grid, object obj) {
        PropertyName = propertName;
        BuildPropertiesUI(grid, EditablePropertyCollector.GetEditableProperties(obj));
    }
    
    public static void BuildPropertiesUI(Grid grid, List<EditablePropertyDetails> properties) {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(properties);

        var firstGroup = true;
        var currentGroup = string.Empty;
        var row = 0;

        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Absolute) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        foreach (var prop in properties) {
            currentGroup = CheckAndCreateGroup(grid, currentGroup, prop, ref firstGroup, ref row);

            // Property Input
            Microsoft.Maui.Controls.View? inputField = CreateInputField(prop);

            if (inputField is not null) {

                // Property Name
                var nameLabel = new Label {
                    Text = prop.Info.Name,
                    Style = ApplyStyle("MediumLabel"),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                };

                // Property Description
                var descriptionLabel = new Label {
                    Text = prop.Attribute.Description,
                    Style = ApplyStyle("SmallLabel"),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                };
                
                // Add Name and Description to column 1
                var stackLayout = new StackLayout {
                    Spacing = 0,
                };

                stackLayout.Children.Add(nameLabel);
                if (!string.IsNullOrEmpty(prop.Attribute.Description)) stackLayout.Children.Add(descriptionLabel);
                
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.SetColumn(stackLayout, 0);
                grid.SetRow(stackLayout, row);
                grid.Children.Add(stackLayout);

                // Add Input Field to column 2
                grid.SetColumn(inputField, 2);
                grid.SetRow(inputField, row);
                grid.Children.Add(inputField);
                
                row++;
            }
        }
    }

    private static string CheckAndCreateGroup(Grid grid, string currentGroup, EditablePropertyDetails prop, ref bool firstGroup, ref int row) {
        // Group Header: Only if we have a group name and it is not blank
        if (!string.IsNullOrEmpty(currentGroup) && prop.Attribute.Group != currentGroup) {
            currentGroup = prop.Attribute.Group;

            var groupLabel = new Label {
                Text = currentGroup,
                Style = ApplyStyle("LargeLabel"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            // Blank line between groups
            if (!firstGroup) {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                row++;
            }

            grid.Margin = new Thickness(5, 10, 5, 10);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.SetColumn(groupLabel, 0);
            grid.SetRow(groupLabel, row);
            Grid.SetColumnSpan(groupLabel, 2);
            grid.Children.Add(groupLabel);
            row++;
            firstGroup = false;
        }

        return currentGroup;
    }

    private static Style? ApplyStyle(string styleName) {
        if (Application.Current is { } app && 
            app.Resources.TryGetValue(styleName, out var style) 
            && style is not null) {
            return (Style)style;
        }
        return null;
    }

    private static Microsoft.Maui.Controls.View? CreateInputField(EditablePropertyDetails prop) {
        Microsoft.Maui.Controls.View inputField = null;

        if (prop.Type == typeof(string)) {
            inputField = new Entry {
                Text = prop.Info.GetValue(prop.Owner)?.ToString() ?? string.Empty,
                HorizontalOptions = LayoutOptions.Start,
                Style = ApplyStyle("Entry"),
                ClearButtonVisibility = ClearButtonVisibility.Never,
            };
        }
        else if (prop.Type == typeof(int)) {
            inputField = new Entry {
                Text = prop.Info.GetValue(prop.Owner)?.ToString() ?? "0",
                Keyboard = Keyboard.Numeric,
                HorizontalOptions = LayoutOptions.Start,
                ClearButtonVisibility = ClearButtonVisibility.Never,
                Style = ApplyStyle("Entry")
            };
        }
        // Add more types as needed, e.g., Color, Enum, etc.

        // Attach a value-changed handler if needed to update the property
        if (inputField is Entry entry) {
            entry.TextChanged += (sender, args) => {
                var value = args.NewTextValue;

                if (prop.Type == typeof(int) && int.TryParse(value, out int intValue)) {
                    prop.Info.SetValue(prop.Owner, intValue);
                }
                else if (prop.Type == typeof(string)) {
                    prop.Info.SetValue(prop.Owner, value);
                }
            };
        }
        return inputField;
    }
}    