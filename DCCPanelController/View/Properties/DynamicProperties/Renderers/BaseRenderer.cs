using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View.Properties.DynamicProperties;

public abstract class BaseRenderer {
    protected virtual int LabelWidth => 150; // Allow this to be overriden
    protected virtual int FieldHeight => 35;
    protected virtual int FieldWidth => -1;

    protected virtual int LabelFontSize => 15;
    protected virtual int FieldFontSize => 15;
    protected virtual int ErrorFontSize => 12;
    protected virtual int DescFontSize => 10;
    protected virtual int ColumnSpacing => 12;
    protected virtual int RowSpacing => 2;

    protected static readonly Color LabelColor = Colors.Black;
    protected static readonly Color FieldColor = Colors.Black;
    protected static readonly Color ErrorColor = Colors.Red;
    protected static readonly Color DescColor = Colors.DarkGray;
    protected static readonly Color ModifiedColor = Colors.DarkOrange;
    protected static readonly Color MixedValueColor = Colors.Blue;

    protected bool IsModified(PropertyRow row) => row.IsTouched || !DefaultEquality.AreEqual(row.CurrentValue, row.OriginalValue);
    protected string MixedPlaceholder(PropertyRow row) => row.HasMixedValues ? "— mixed —" : string.Empty;

    protected event Action<PropertyRow>? RowValueChanged;

    protected void SetValue(PropertyRow row, object? newVal) {
        row.IsTouched = true;
        row.CurrentValue = newVal;
        RowValueChanged?.Invoke(row); // <— notify UI wrappers to refresh visuals
    }

    protected int GetFieldWidth(int? fieldWidth) {
        // Need to account for iPhone style displays so set a maximum width that we can 
        // adjust the field to. If we had the field as 300 + 150 label then 450 is > iPhone width
        // ---------------------------------------------------------------------------------------
        var maxWidth = FieldWidth >= 0 ? FieldWidth : 300;
        var page = App.Current.Windows[0].Page;
        if (page is not null) {
            maxWidth = (int)page.Width;
            maxWidth -= (LabelWidth + ColumnSpacing) + 20;
        }

        return fieldWidth switch {
            -1                       => maxWidth,
            0 when FieldWidth is -1  => maxWidth,
            0 when FieldWidth is > 0 => Math.Min(FieldWidth, maxWidth),
            > 0                      => Math.Min(fieldWidth.Value, maxWidth),
            _                        => Math.Min(FieldWidth, maxWidth)
        };
    }

    protected Microsoft.Maui.Controls.View AddBorder(Microsoft.Maui.Controls.View view) {
        var border = new Border {
            Stroke = new SolidColorBrush(Colors.DarkGray),
            StrokeThickness = 1,
            Background = new SolidColorBrush(Colors.White),
            StrokeShape = new RoundRectangle {
                CornerRadius = new CornerRadius(6) // uniform radius
            },
            Content = view,
        };
        return border;
    }

    protected Microsoft.Maui.Controls.View WrapWithLabel(PropertyContext ctx,
                                                         Microsoft.Maui.Controls.View control) {
        var row = ctx.Row;
        var editorKind = ctx.EditorKind;
        var labelWidth = LabelWidth;
        var fieldHeight = FieldHeight;
        var fieldWidth = GetFieldWidth(row?.Field?.Meta?.Width);

        var label = new Label {
            Text = row?.Field?.Meta?.Label ?? "Unknown",
            FontSize = LabelFontSize,
            TextColor = LabelColor,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            HorizontalOptions = LayoutOptions.Fill,
        };

        var descriptionKey = row?.Field?.Meta?.Description ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(descriptionKey)
            ? null
            : new Label {
                Text = descriptionKey,
                TextColor = DescColor,
                FontSize = DescFontSize, Opacity = 0.7
            };

        var errorLabel = new Label {
            FontSize = ErrorFontSize,
            TextColor = ErrorColor,
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
                new RowDefinition(fieldHeight >= 0 ? fieldHeight : GridLength.Auto),    // label + control
                new RowDefinition(GridLength.Auto) // description + error under control
            },
            ColumnSpacing = ColumnSpacing,
            RowSpacing = RowSpacing
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

        RowValueChanged += OnRowValueChanged;
        grid.Unloaded += (_, __) => RowValueChanged -= OnRowValueChanged;

        RefreshVisuals();
        return grid;

        // Listen for value changes on this row (so label updates as you type/click)
        void OnRowValueChanged(PropertyRow changed) {
            if (ReferenceEquals(changed, row)) RefreshVisuals();
        }

        void RefreshVisuals() {
            if (row is null) return;
            var modified = IsModified(row);
            label.TextColor = modified ? ModifiedColor
                : (row.HasMixedValues) ? MixedValueColor : LabelColor;
            ;

            var err = row.Issues.FirstOrDefault(i => i.Severity == Severity.Error);
            errorLabel.IsVisible = err != null;
            errorLabel.Text = err?.Message ?? string.Empty;
        }
    }
}