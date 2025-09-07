namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class DateRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Date;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var dp = new DatePicker();
        if (row.OriginalValue is DateTime dt) dp.Date = dt;
        dp.DateSelected += (s, e) => RenderBinding.SetValue(row, e.NewDate);
        dp.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, dp);
    }
}
