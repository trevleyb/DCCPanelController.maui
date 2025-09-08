namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class TextRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Text;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { Text = row.OriginalValue as string ?? string.Empty, Placeholder = MixedPlaceholder(row) };
        entry.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(entry));
    }
}
