namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class TextRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Text;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Text = row.OriginalValue as string, Placeholder = RenderBinding.MixedPlaceholder(row) };
        entry.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, entry);
    }
}
