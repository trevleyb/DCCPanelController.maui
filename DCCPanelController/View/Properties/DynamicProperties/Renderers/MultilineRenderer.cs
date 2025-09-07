namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class MultilineTextRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Multiline;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var editor = new Editor { Text = row.OriginalValue as string, AutoSize = EditorAutoSizeOption.TextChanges, Placeholder = RenderBinding.MixedPlaceholder(row), HeightRequest = 120 };
        editor.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        editor.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, editor);
    }
}
