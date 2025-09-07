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
        registry.Register("enum", new EnumRadioRenderer());
        registry.Register("enum.choice", new EnumChoiceRenderer());
        registry.Register("enum.radio",  new EnumRadioRenderer());

        registry.Register("actions", new ActionsRenderer());
        registry.Register("block", new BlockRenderer());
        registry.Register("button", new ButtonRenderer());
        registry.Register("image", new ImageRenderer());
        registry.Register("switch", new SwitchRenderer());
        registry.Register("opacity", new OpacityRenderer());
        registry.Register("route", new RouteRenderer());
        registry.Register("turnout", new TurnoutRenderer());
    }

    public static Microsoft.Maui.Controls.View WrapWithLabel(
        PropertyRow row,
        Microsoft.Maui.Controls.View control) {

        var editorKind = row.Field?.Meta?.EditorKind;
        var labelWidth = EditorKinds.LabelWidth(editorKind);
        var fieldWidth = row?.Field?.Meta?.Width ?? EditorKinds.FieldWidth(editorKind);
        var fieldHeight = EditorKinds.FieldHeight(editorKind);
        var columnSpacing = 12;
        var rowSpacing = 2;
        
        var label = new Label {
            Text = row?.Field?.Meta?.Label ?? "Unknown",
            FontSize = 15,
            TextColor = Colors.Black,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            HorizontalOptions = LayoutOptions.Fill,
        };

        var descriptionKey = row?.Field?.Meta?.Description ?? string.Empty;
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
                new ColumnDefinition(labelWidth),
                new ColumnDefinition(fieldWidth >= 0 ? fieldWidth : GridLength.Star)
            },
            RowDefinitions = {
                new RowDefinition(fieldHeight), // label + control
                new RowDefinition(GridLength.Auto)  // description + error under control
            },
            ColumnSpacing = columnSpacing,
            RowSpacing = rowSpacing
        };

        // Row 0: label + control
        // -----------------------------------------------------------------
        grid.Add(label, 0, 0);
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