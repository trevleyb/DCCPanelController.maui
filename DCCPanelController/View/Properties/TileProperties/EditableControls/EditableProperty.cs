using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

[AttributeUsage(AttributeTargets.Property)]
public abstract class EditableProperty(string label, string description = "", int order = 0, string? group = null) : Attribute, INotifyPropertyChanged {
    
    public string Label { get; } = label;              // Label/Prompt for the property
    public string Description { get; } = description;  // Description of the property
    public string Group { get; } = group ?? "General"; // Group to which this property belongs
    public int Order { get; } = order;                 // Order within the group
    public object? Value { get; set; } = null;         // Initial Value or display Value
    public bool IsModified { get; set; } = false;      // Has this been modified?
    
    protected IView CreateGroupCell(IView view, int? height = null) {
        var grid = new Grid();
        grid.MinimumHeightRequest = height ?? 43;
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 150 });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = 3 });
        grid.BackgroundColor = Colors.White;

        // If there is a Label, then add the label to the first column. 
        // -----------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(Label)) {
            var stack = new VerticalStackLayout();
            stack.VerticalOptions = LayoutOptions.Center;
            
            var label = new Label {
                Text = Label,
                FontSize = 15,
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 5, 5)
            };
            label.SetBinding(Microsoft.Maui.Controls.Label.TextColorProperty, new Binding(nameof(ModifiedTextColor)) { Source = this, Mode = BindingMode.OneWay });
            stack.Children.Add(label);

            // If there is a description to go with the lqbel, add it under the label
            // -----------------------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(Description)) {
                var desc = new Label {
                    Text = Description,
                    FontSize = 8,
                    FontFamily = "OpenSansLight",
                    LineBreakMode = LineBreakMode.MiddleTruncation,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(5, -5, 5, 0)
                };
                stack.Children.Add(desc);
            }
            grid.Children.Add(stack);
            grid.SetColumn(stack, 0);
            grid.SetRow(stack, 0);
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
        IsModified = false;
        return grid;
    }

    protected void SetModified(bool isModified) {
        IsModified = isModified;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(ModifiedTextColor));
    }
    public Color ModifiedTextColor => IsModified ? Colors.Firebrick : Colors.DimGray;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}