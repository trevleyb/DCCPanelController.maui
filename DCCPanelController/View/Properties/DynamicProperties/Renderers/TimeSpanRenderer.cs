using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class TimeSpanRenderer : BaseRenderer, IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.TimeSpan;

    public object CreateView(PropertyContext ctx) {
        try {
        var row = ctx.Row;
        var entry = new Entry { Placeholder = row.HasMixedValues ? "— mixed —" : "hh:mm:ss", TextColor = Colors.Black, };
        if (row.OriginalValue is TimeSpan ts) entry.Text = ts.ToString();
        entry.TextChanged += (s, e) => {
            if (TimeSpan.TryParse(e.NewTextValue, out var v)) SetValue(row, v);
        };
        entry.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, entry);
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating Time Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new InvalidRenderer(ex.Message).CreateView(ctx);
        }
    }
}