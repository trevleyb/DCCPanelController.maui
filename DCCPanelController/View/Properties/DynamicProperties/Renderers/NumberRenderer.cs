namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class NumberRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 100;

    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Number;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var entry = new Entry {
            Keyboard = Keyboard.Numeric, 
            Text = row.OriginalValue?.ToString() ?? "0", Placeholder = MixedPlaceholder(row),
            Margin=new Thickness(5,0,5,0),
        };
        entry.Behaviors.Add(new CommunityToolkit.Maui.Behaviors.NumericValidationBehavior());
        entry.TextChanged += (s, e) => {
            if (double.TryParse(e.NewTextValue, out var v)) SetValue(row, v);
        };
        entry.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(entry));
    }
}
