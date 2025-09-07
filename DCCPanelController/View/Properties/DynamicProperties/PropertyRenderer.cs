using System.Text.RegularExpressions;
using DCCPanelController.View.Components;

namespace DCCPanelController.View.Properties.DynamicProperties;

internal static class RenderBinding {
    public static event Action<PropertyRow>? RowValueChanged;
    public static void SetValue(PropertyRow row, object? newVal) {
        row.IsTouched = true;
        row.CurrentValue = newVal;
        RowValueChanged?.Invoke(row);   // <— notify UI wrappers to refresh visuals
    }
    
    public static bool IsModified(PropertyRow row) => row.IsTouched || !DefaultEquality.AreEqual(row.CurrentValue, row.OriginalValue);
    public static string MixedPlaceholder(PropertyRow row) => row.HasMixedValues ? "— mixed —" : string.Empty;
}

public static class PropertyRenderers {
    public static void RegisterDefaults(IPropertyRendererRegistry registry) {
        registry.Register("text", new TextRenderer());
        registry.Register("multiline", new MultilineTextRenderer());
        registry.Register("password", new PasswordRenderer());
        registry.Register("int", new IntRenderer());
        registry.Register("number", new NumberRenderer());
        registry.Register("toggle", new ToggleRenderer());
        registry.Register("choice", new ChoiceRenderer());
        registry.Register("color", new ColorPickerRenderer());
        registry.Register("date", new DateRenderer());
        registry.Register("timespan", new TimeSpanRenderer());
        registry.Register("url", new UrlRenderer());

        // TODO: Need to add support for these types
        registry.Register("actions", new TextRenderer());
        registry.Register("block", new TextRenderer());
        registry.Register("button", new TextRenderer());
        registry.Register("image", new TextRenderer());
        registry.Register("switch", new TextRenderer());
        registry.Register("opacity", new TextRenderer());
        registry.Register("route", new TextRenderer());
        registry.Register("turnout", new TextRenderer());
    }

    public static Microsoft.Maui.Controls.View WrapWithLabel(PropertyRow row,
                                                              Microsoft.Maui.Controls.View control,
                                                              double? fieldColumnWidth = -1,
                                                              double labelColumnWidth = -1, 
                                                              double columnSpacing = 12,
                                                              double rowSpacing = 2) {
        var label = new Label {
            Text = row.Field.Meta.Label,
            FontSize = 15,
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start
        };
        if (labelColumnWidth > 0) label.WidthRequest = labelColumnWidth;

        var descriptionKey = row.Field.Meta.Description;
        var description = string.IsNullOrWhiteSpace(descriptionKey)
            ? null
            : new Label { Text = descriptionKey, TextColor = Colors.Gray, FontSize = 10, Opacity = 0.7 };

        var errorLabel = new Label {
            FontSize = 15,
            TextColor = Colors.Red,
            IsVisible = false
        };
        
        // Create a 2 Column grid, first fixed to 150 and second Auto
        // -----------------------------------------------------------------
        var grid = new Grid {
            ColumnDefinitions = {
                labelColumnWidth > 0 ? new ColumnDefinition(labelColumnWidth) : new ColumnDefinition(150),
                new ColumnDefinition((fieldColumnWidth ?? -1) >= 0 ? fieldColumnWidth ?? GridLength.Auto : GridLength.Auto)
            },
            RowDefinitions = {
                new RowDefinition(50), // label + control
                new RowDefinition(GridLength.Auto)  // description + error under control
            },
            ColumnSpacing = columnSpacing,
            RowSpacing = rowSpacing
        };

        // Row 0: label + control
        // -----------------------------------------------------------------
        grid.Add(label, 0, 0);
        control.VerticalOptions = LayoutOptions.Center;
        control.HorizontalOptions = LayoutOptions.Fill;
        grid.Add(control, 1, 0);

        // Row 1: description/error, indented under the control (col 1)
        // -----------------------------------------------------------------
        var infoStack = new VerticalStackLayout { Spacing = 2 };
        if (description != null) infoStack.Add(description);
        infoStack.Add(errorLabel);
        grid.Add(infoStack, 1, 1);

        RenderBinding.RowValueChanged += OnRowValueChanged;
        grid.Unloaded += (_, __) => RenderBinding.RowValueChanged -= OnRowValueChanged;

        RefreshVisuals();
        return grid;

        // Listen for value changes on this row (so label updates as you type/click)
        void OnRowValueChanged(PropertyRow changed) {
            if (ReferenceEquals(changed, row)) RefreshVisuals();
        }

        void RefreshVisuals() {
            var modified = RenderBinding.IsModified(row);
            label.TextColor = modified ? Colors.Red : (row.HasMixedValues) ? Colors.ForestGreen : Colors.Black;

            var err = row.Issues.FirstOrDefault(i => i.Severity == Severity.Error);
            errorLabel.IsVisible = err != null;
            errorLabel.Text = err?.Message ?? string.Empty;
        }
    }
}