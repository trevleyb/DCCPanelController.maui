namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class MultilineTextRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 250;
    protected override int FieldHeight => 120;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Multiline;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var editor = new Editor { Text = row.OriginalValue as string, AutoSize = EditorAutoSizeOption.TextChanges, Placeholder = MixedPlaceholder(row), HeightRequest = 120,                 Margin=new Thickness(5,0,5,0)
        };
        editor.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        editor.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, editor);
    }
}
