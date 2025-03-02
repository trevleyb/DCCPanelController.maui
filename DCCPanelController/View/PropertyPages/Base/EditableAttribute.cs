using DCCPanelController.Helpers;

namespace DCCPanelController.View.PropertyPages.Base;

[AttributeUsage(AttributeTargets.Property)]
public abstract class EditableAttribute : Attribute {
    public string Name { get; set; } = string.Empty;        // SystemName to show on the Properties Page
    public string Description { get; set; } = string.Empty; // Description to show under/next to the Property
    public string Group { get; set; } = string.Empty;       // Group Identifier. Used to group items in a box
    public int Order { get; set; } = 0;                     // What is the Sort order. If 0, then by order in the class

    protected Label CreateUndefined(EditableDetails value, string text = "Not yet defined") {
        return new Label {
            Text = text,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };
    }

    protected StackLayout CreateRadioGroupForEnums<T>(string name, T[] items, object source, string fieldName) where T : struct, Enum {
        if (source == null) throw new ArgumentNullException(nameof(source), "Binding source cannot be null.");
        if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));

        var radioGroup = new StackLayout {
            HeightRequest = 30,
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(-5, 0, 0, 0)
        };

        foreach (var value in items) {
            var radioButton = new RadioButton {
                HeightRequest = 30,
                BorderWidth = 0,
                FontSize = 12,
                Content = value.ToString() // Display the value
            };

            radioButton.CheckedChanged += (sender, args) => { PropertyHelper.SetEnumPropertyValue(source, fieldName, value); };
            radioButton.IsChecked = value.Equals(PropertyHelper.GetEnumPropertyValue<T>(source, fieldName));
            radioGroup.Children.Add(radioButton);
        }

        return radioGroup;
    }
}