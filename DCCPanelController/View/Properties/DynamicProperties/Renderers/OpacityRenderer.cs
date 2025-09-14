using System.Globalization;
using CommunityToolkit.Maui.Behaviors;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class OpacityRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 175;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Opacity;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var min = 0;
        var max = 1;
        var step = 0.05;

        var stepperWidth = 100;
        var grid = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(stepperWidth)] };
        var entry = new Entry {
            Keyboard = Keyboard.Numeric,
            Text = row.OriginalValue != null ? ConvertOpacityToPercentage(row.OriginalValue) : string.Empty,
            Placeholder = MixedPlaceholderInt(row),
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.End,
            Margin = new Thickness(5, 0, 5, 0),
        };
        var stepper = new Stepper {
            Minimum = min, Maximum = max, Increment = step, Margin = new Thickness(10, 0, 0, 0),
            Value = row.OriginalValue is double value ? value : 0,
        };

        stepper.ValueChanged += (s, e) => {
            var val = Math.Clamp(stepper.Value, min, max);
            entry.Text = ConvertOpacityToPercentage(val);
            SetValue(row, val);
        };

        entry.Behaviors.Add(new NumericValidationBehavior());
        entry.TextChanged += (s, e) => {
            var val = ConvertToPercentageToOpacity(e.NewTextValue);
            val = Math.Clamp(val, min, max);
            SetValue(row, val);
        };
        grid.Add(AddBorder(entry), 0);
        grid.Add(stepper, 1);
        grid.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, grid);
    }

    private static string ConvertOpacityToPercentage(object? opacity) {
        if (opacity is double d) return ConvertOpacityToPercentage(d);
        return ConvertOpacityToPercentage(0);
    }

    private static string ConvertOpacityToPercentage(double opacity) => opacity.ToString("0%", CultureInfo.InvariantCulture);

    private static double ConvertToPercentageToOpacity(string percentage) {
        if (percentage.EndsWith("%") && double.TryParse(percentage.TrimEnd('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue)) {
            return Math.Clamp(parsedValue / 100.0, 0.0, 1.0);
        }
        Console.WriteLine($"Invalid percentage format: {percentage}. Ensure the value ends with '%' and represents a valid number.");
        return 0.0;
    }
}