namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class ChoiceRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Choice;

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var picker = new Picker {
            Title = (row.HasMixedValues ? "— mixed —" : null) ?? string.Empty,
            FontSize = 12,
            HeightRequest = 30
        };
        if (ctx.Params.TryGetValue("choices", out var itemsObj) && itemsObj is IEnumerable<string> items) {
            foreach (var i in items) picker.Items.Add(i);
        }
        if (row.OriginalValue is string s && picker.Items.Contains(s)) picker.SelectedItem = s;
        picker.SelectedIndexChanged += (s2, e2) => RenderBinding.SetValue(row, picker.SelectedItem);
        picker.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, picker);
    }
}
