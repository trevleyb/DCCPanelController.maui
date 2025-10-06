namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class PasswordRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 250;

    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Password;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry {
            IsPassword = true,
            TextColor = Colors.Black,
            Text = row.OriginalValue as string, Placeholder = MixedPlaceholder(row),
            Margin = new Thickness(5, 0, 5, 0),
        };
        entry.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        entry.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, AddBorder(entry));
    }
}