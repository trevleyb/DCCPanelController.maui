namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class TimeSpanRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TimeSpan;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Placeholder = row.HasMixedValues ? "— mixed —" : "hh:mm:ss" };
        if (row.OriginalValue is TimeSpan ts) entry.Text = ts.ToString();
        entry.TextChanged += (s, e) => {
            if (TimeSpan.TryParse(e.NewTextValue, out var v)) SetValue(row, v);
        };
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, entry);
    }
}