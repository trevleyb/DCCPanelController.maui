namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class DateRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 150;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Date;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var dp = new DatePicker {
            TextColor = Colors.Black,
            FontSize = FieldFontSize,
            FontAttributes = FontAttributes.None,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            Format = "D",
        };
        if (row.OriginalValue is DateTime dt) dp.Date = dt;
        dp.DateSelected += (s, e) => SetValue(row, e.NewDate);
        dp.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, AddBorder(dp));
    }
}