namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class UrlRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Url;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Keyboard = Keyboard.Url, Text = row.OriginalValue?.ToString(), Placeholder = row.HasMixedValues ? "— mixed —" : "https://" };
        entry.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, entry);
    }
}