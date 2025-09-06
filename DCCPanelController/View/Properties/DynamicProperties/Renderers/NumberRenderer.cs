namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class NumberRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "number";
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Keyboard = Keyboard.Numeric, Text = row.OriginalValue?.ToString(), Placeholder = RenderBinding.MixedPlaceholder(row) };
        entry.TextChanged += (s, e) => {
            if (double.TryParse(e.NewTextValue, out var v)) RenderBinding.SetValue(row, v);
        };
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, entry, 100);
    }
}
