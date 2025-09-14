namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ToggleRenderer : BaseRenderer, IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Toggle;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var sw = new Switch { IsToggled = row.OriginalValue as bool? ?? false };
        if (row.HasMixedValues) {
            var stack = new Grid();
            var label = new Label { Text = "— mixed —", VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Start, Opacity = 0.6 };
            stack.Add(sw);
            stack.Add(label);
            sw.Toggled += (s, e) => {
                label.IsVisible = false;
                SetValue(row, e.Value);
            };
            stack.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
            return WrapWithLabel(ctx, stack);
        }
        sw.Toggled += (s, e) => SetValue(row, e.Value);
        sw.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, sw);
    }
}