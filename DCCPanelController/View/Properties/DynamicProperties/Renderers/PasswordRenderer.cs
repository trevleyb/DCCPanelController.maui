namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class PasswordRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "password";
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { IsPassword = true, Text = row.OriginalValue as string, Placeholder = RenderBinding.MixedPlaceholder(row) };
        entry.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, entry, 250);
    }
}
