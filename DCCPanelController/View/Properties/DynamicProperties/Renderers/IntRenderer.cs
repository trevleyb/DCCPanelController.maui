using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

    internal sealed class IntRenderer : IPropertyRenderer {
        public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Int;
        public object CreateView(PropertyContext ctx) {
            var row = ctx.Row;
            var min = row.Field.Meta.GetParameters("min", int.MinValue);
            var max = row.Field.Meta.GetParameters("max", int.MaxValue);
            var step = row.Field.Meta.GetParameters("step", 1);

            var grid = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto)] };
            var entry = new Entry {Keyboard = Keyboard.Numeric, Text = row.OriginalValue?.ToString() ?? string.Empty, Placeholder = RenderBinding.MixedPlaceholder(row), HorizontalOptions = LayoutOptions.Fill, HorizontalTextAlignment = TextAlignment.End };
            var stepper = new Stepper { Value=(int)(row.OriginalValue ?? 0.0),  Minimum = min, Maximum = max, Increment = step, Margin=new Thickness(10,0,0,0) };

            stepper.ValueChanged += (s, e) => {
                var val = Math.Clamp(stepper.Value, min, max);
                entry.Text = val.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                RenderBinding.SetValue(row, val);
            };
            
            entry.TextChanged += (s, e) => {
                if (int.TryParse(e.NewTextValue, out var v)) {
                    v = Math.Clamp(v, min, max);
                    RenderBinding.SetValue(row, v);
                }
            };
            grid.Add(entry, 0, 0);
            grid.Add(stepper, 1, 0);

            grid.Add(entry, 0, 0);
            grid.Add(stepper, 1, 0);
            grid.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
            return PropertyRenderers.WrapWithLabel(row, grid);
        }
    }
