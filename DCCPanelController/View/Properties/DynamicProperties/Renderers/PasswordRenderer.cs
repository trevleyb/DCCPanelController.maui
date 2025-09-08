namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class PasswordRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Password;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry { IsPassword = true, Text = row.OriginalValue as string, Placeholder = MixedPlaceholder(row) };
        entry.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(entry));
    }
}
