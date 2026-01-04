using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class InvalidRenderer(string message) : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => true;

    public object CreateView(PropertyContext ctx) {
        try {
        var label = new Label {
            Text = message,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Start,
            Opacity = 0.6,
            TextColor = ErrorColor,
            Margin = new Thickness(5, 0, 5, 0),
        };
        return WrapWithLabel(ctx, AddBorder(label));
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating Invalid Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new Label { Text = "Error creating property control" };
        }
    }
}