using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCCPanelController.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

[AttributeUsage(AttributeTargets.Property)]
public abstract class EditableProperty(string label, string description = "", int order = 0, string? group = null) : Attribute, INotifyPropertyChanged {
    public string Label { get; } = label;              // Label/Prompt for the property
    public string Description { get; } = description;  // Description of the property
    public string Group { get; } = group ?? "General"; // Group to which this property belongs
    public int Order { get; } = order;                 // Order within the group
    public object? Value { get; set; } = null;         // Initial Value or display Value
    public bool IsModified { get; set; } = false;      // Has this been modified?
    public bool HasMixedValues { get; set; } = false; 
    
    protected ILogger<EditableProperty> PropertyLogger = LogHelper.CreateLogger<EditableProperty>();
    
    protected IView CreateGroupCell(IView? view, int? height = null) {

        var vStack = new VerticalStackLayout();
        var hStack = new HorizontalStackLayout();

        // If there is a Label, then add the label to the first column. 
        // -----------------------------------------------------------------------------
        var label = new Label {
            Text = Label,
            FontSize = 15,
            LineBreakMode = LineBreakMode.MiddleTruncation,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 150,
            Margin = new Thickness(5, 5, 5, 5)
        };
        label.SetBinding(Microsoft.Maui.Controls.Label.TextColorProperty, new Binding(nameof(ModifiedTextColor)) { Source = this, Mode = BindingMode.OneWay });
        hStack.Children.Add(label);
        if (view is not null) hStack.Children.Add(view);
        vStack.Children.Add(hStack);
        
        // If there is a description to go with the lqbel, add it under the label
        // -----------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(Description)) {
            var desc = new Label {
                Text = Description,
                FontSize = 10,
                FontFamily = "OpenSansLight",
                LineBreakMode = LineBreakMode.MiddleTruncation,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 0, 5, 0)
            };
            vStack.Children.Add(desc);
        }
        
        // Add a divider line into the 2nd row and span across both columns
        // -----------------------------------------------------------------------------
        var boxView = new BoxView {
            BackgroundColor = Colors.Gainsboro,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(5, 5, 5, 5)
        };
        vStack.Children.Add(boxView);
        IsModified = false;
        return vStack;
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