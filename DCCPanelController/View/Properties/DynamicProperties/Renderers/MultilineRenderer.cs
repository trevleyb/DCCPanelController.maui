using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class MultilineTextRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 250;
    protected override int FieldHeight => 120;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Multiline;

    public object CreateView(PropertyContext ctx) {
        try {
        var row = ctx.Row;
        var editor = new Editor {
            TextColor = Colors.Black,
            Text = row.OriginalValue as string, AutoSize = EditorAutoSizeOption.TextChanges, Placeholder = MixedPlaceholder(row), HeightRequest = 120, Margin = new Thickness(5, 0, 5, 0),
        };
        editor.TextChanged += (s, e) => SetValue(row, e.NewTextValue);
        editor.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, editor);
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating MultiLine Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new InvalidRenderer(ex.Message).CreateView(ctx);
        }
    }
}