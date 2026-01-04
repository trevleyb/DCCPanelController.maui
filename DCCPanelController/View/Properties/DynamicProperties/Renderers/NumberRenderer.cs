using CommunityToolkit.Maui.Behaviors;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class NumberRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 75;

    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Number;

    public object CreateView(PropertyContext ctx) {
        try {
        var row = ctx.Row;
        var entry = new Entry {
            TextColor = Colors.Black,
            Keyboard = Keyboard.Numeric,
            Text = row.OriginalValue?.ToString() ?? "0", Placeholder = MixedPlaceholder(row),
            Margin = new Thickness(5, 0, 5, 0),
        };
        entry.Behaviors.Add(new NumericValidationBehavior());
        entry.TextChanged += (s, e) => {
            if (double.TryParse(e.NewTextValue, out var v)) SetValue(row, v);
        };
        entry.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, AddBorder(entry));
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating Number Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new InvalidRenderer(ex.Message).CreateView(ctx);
        }
    }
}