namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ActionsTurnoutRenderer : BaseRenderer, IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TurnoutActions;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var label = new Label { Text = "Actions", VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Start, Opacity = 0.6 };
        //var entry = new Entry { Text = row.OriginalValue as string, Placeholder = RenderBinding.MixedPlaceholder(row) };
        //entry.TextChanged += (s, e) => RenderBinding.SetValue(row, e.NewTextValue);
        //entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, label);
    }
}