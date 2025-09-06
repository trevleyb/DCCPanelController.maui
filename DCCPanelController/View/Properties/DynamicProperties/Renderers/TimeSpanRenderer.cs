namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class TimeSpanRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "timespan";
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Placeholder = row.HasMixedValues ? "— mixed —" : "hh:mm:ss" };
        if (row.OriginalValue is TimeSpan ts) entry.Text = ts.ToString();
        entry.TextChanged += (s, e) => {
            if (TimeSpan.TryParse(e.NewTextValue, out var v)) RenderBinding.SetValue(row, v);
        };
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, entry, 150);
    }
}