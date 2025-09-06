namespace DCCPanelController.View.Properties.DynamicProperties;

    internal sealed class IntRenderer : IPropertyRenderer {
        public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "int";
        public object CreateView(PropertyContext ctx) {
            var row = ctx.Row;
            var min = row.Field.Meta.GetParameters("min", int.MinValue);
            var max = row.Field.Meta.GetParameters("max", int.MaxValue);
            var step = row.Field.Meta.GetParameters("step", 1);
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
            var entry = new Entry { Keyboard = Keyboard.Numeric, Text = row.OriginalValue?.ToString(), Placeholder = RenderBinding.MixedPlaceholder(row), HorizontalOptions = LayoutOptions.Fill };
            var minus = new Button { Text = "−" };
            var plus = new Button { Text = "+" };
            minus.Clicked += (s, e) => {
                var val = Parse(entry.Text, (int?)row.OriginalValue ?? 0);
                val = Math.Clamp(val - step, min, max);
                entry.Text = val.ToString();
                RenderBinding.SetValue(row, val);
            };
            plus.Clicked += (s, e) => {
                var val = Parse(entry.Text, (int?)row.OriginalValue ?? 0);
                val = Math.Clamp(val + step, min, max);
                entry.Text = val.ToString();
                RenderBinding.SetValue(row, val);
            };
            entry.TextChanged += (s, e) => {
                if (int.TryParse(e.NewTextValue, out var v)) {
                    v = Math.Clamp(v, min, max);
                    RenderBinding.SetValue(row, v);
                }
            };
            grid.Add(entry, 0, 0);
            grid.Add(minus, 1, 0);
            grid.Add(plus, 2, 0);
            grid.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
            return PropertyRenderers.WrapWithLabel(row, grid, 100);
            static int Parse(string? s, int fallback) => int.TryParse(s, out var v) ? v : fallback;
        }
    }
