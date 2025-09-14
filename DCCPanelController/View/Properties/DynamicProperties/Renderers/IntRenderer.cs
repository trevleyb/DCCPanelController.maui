using System.Globalization;
using CommunityToolkit.Maui.Behaviors;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class IntRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 175;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Int;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var min = (int)row.Field.Meta.Min;
        var max = (int)row.Field.Meta.Max;
        var step = (int)row.Field.Meta.Step;

        var stepperWidth = 100;

        var grid = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(stepperWidth)] };
        var entry = new Entry {
            Keyboard = Keyboard.Numeric,
            Text = row.OriginalValue?.ToString() ?? string.Empty,
            Placeholder = MixedPlaceholderInt(row),
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.End,
            Margin = new Thickness(5, 0, 5, 0),
        };
        var stepper = new Stepper {
            Minimum = min, Maximum = max, Increment = step, Margin = new Thickness(10, 0, 0, 0),
            Value = row.OriginalValue is int value ? value : 0,
        };
        stepper.ValueChanged += (s, e) => {
            var val = (int)Math.Clamp(stepper.Value, min, max);
            entry.Text = val.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            SetValue(row, val);
        };
        entry.Behaviors.Add(new NumericValidationBehavior());
        entry.TextChanged += (s, e) => {
            if (int.TryParse(e.NewTextValue, out var v)) {
                v = Math.Clamp(v, min, max);
                SetValue(row, v);
            }
        };
        grid.Add(AddBorder(entry), 0);
        grid.Add(stepper, 1);
        grid.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, grid);
    }
}