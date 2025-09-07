namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class UrlRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Url;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Keyboard = Keyboard.Url, Text = row.OriginalValue?.ToString(), Placeholder = row.HasMixedValues ? "— mixed —" : "https://" };
        entry.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, entry);
    }
}