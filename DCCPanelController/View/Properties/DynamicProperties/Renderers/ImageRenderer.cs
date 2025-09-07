namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class ImageRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Image;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var label = new Label { Text = "Image", VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Start, Opacity = 0.6 };
        //var entry = new Entry { Text = row.OriginalValue as string, Placeholder = RenderBinding.MixedPlaceholder(row) };
        //entry.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        //entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, label);
    }
}