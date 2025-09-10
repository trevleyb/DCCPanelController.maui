namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class ChoiceRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 250;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Choice;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var picker = new Picker {
            // BUG: If you have title, it doesn't work.
            //Title = (row.HasMixedValues ? "— mixed —" : null) ?? row.Field.Meta.Label,
            WidthRequest = GetFieldWidth(row.Field.Meta.Width),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 0, 5, 0)

        };
        if (ctx.Row?.Field?.Meta?.Choices is { } choices) {
            foreach (var i in choices) picker.Items.Add(i);
        }
        if (row.OriginalValue is string s && picker.Items.Contains(s)) picker.SelectedItem = s;
        picker.SelectedIndexChanged += (s2, e2) => SetValue(row, picker.SelectedItem);
        picker.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(picker));
    }
}
