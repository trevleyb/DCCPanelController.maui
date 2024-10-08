using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers.Attributes;
using Microsoft.Maui.Controls;

namespace DCCPanelController.ViewModel;

public partial class PropertyViewModel : BaseViewModel {

    public PropertyViewModel(Grid grid, object obj) {
        BuildPropertiesUI(grid, EditablePropertyCollector.GetEditableProperties(obj));
    }
    
    public static void BuildPropertiesUI(Grid grid, List<EditablePropertyDetails> properties) {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(properties);

        bool firstGroup = true;
        var currentGroup = string.Empty;
        var row = 0;

        grid.Children.Clear();
        foreach (var prop in properties) {
            // Group Header
            // Only if we have a group name and it is not blank
            if (!string.IsNullOrEmpty(currentGroup) && prop.Attribute.Group != currentGroup) {
                currentGroup = prop.Attribute.Group;

                var groupLabel = new Label {
                    Text = currentGroup,
                    Style = (Style)Application.Current?.Resources["LargeLabel"],
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

            // Property Name
            var nameLabel = new Label {
                Text = prop.PropertyInfo.Name,
                Style = (Style)Application.Current?.Resources["MediumLabel"],
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            // Property Description
            var descriptionLabel = new Label {
                Text = prop.Attribute.Description,
                Style = (Style)Application.Current?.Resources["SmallLabel"],
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
            };

            // Property Input
            Microsoft.Maui.Controls.View inputField = CreateInputField(prop);

            if (inputField is not null) {

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
                grid.SetColumn(inputField, 1);
                grid.SetRow(inputField, row);
                grid.Children.Add(inputField);
            }

            row++;
        }
    }

    private static Microsoft.Maui.Controls.View? CreateInputField(EditablePropertyDetails prop) {
        Microsoft.Maui.Controls.View inputField = null;

        if (prop.PropertyType == typeof(string)) {
            inputField = new Entry {
                Text = prop.PropertyInfo.GetValue(prop.PropertyOwner)?.ToString() ?? string.Empty,
                Style = (Style)Application.Current?.Resources["PanelEntry"],
                HorizontalOptions = LayoutOptions.Start
            };
        }
        else if (prop.PropertyType == typeof(int)) {
            inputField = new Entry {
                Text = prop.PropertyInfo.GetValue(prop.PropertyOwner)?.ToString() ?? "0",
                Keyboard = Keyboard.Numeric,
                Style = (Style)Application.Current?.Resources["PanelEntry"],
                HorizontalOptions = LayoutOptions.Start
            };
        }
        // Add more types as needed, e.g., Color, Enum, etc.

        // Attach a value-changed handler if needed to update the property
        if (inputField is Entry entry) {
            entry.TextChanged += (sender, args) => {
                var value = args.NewTextValue;

                if (prop.PropertyType == typeof(int) && int.TryParse(value, out int intValue)) {
                    prop.PropertyInfo.SetValue(prop.PropertyOwner, intValue);
                }
                else if (prop.PropertyType == typeof(string)) {
                    prop.PropertyInfo.SetValue(prop.PropertyOwner, value);
                }
            };
        }
        return inputField;
    }
}    