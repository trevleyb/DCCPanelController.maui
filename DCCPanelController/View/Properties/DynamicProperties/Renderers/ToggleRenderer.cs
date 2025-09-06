namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class ToggleRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "toggle";
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
                RenderBinding.SetValue(row, e.Value);
            };
            stack.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
            return PropertyRenderers.WrapWithLabel(row, stack, 100);
        }
        sw.Toggled += (s, e) => RenderBinding.SetValue(row, e.Value);
        sw.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, sw, 100);
    }
}
