using DCCPanelController.View.Components;
using DCCPanelController.View.Helpers;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class FontAliasRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 250;
    
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.FontAlias;

    public object CreateView(PropertyContext ctx) {
        try {
        var row = ctx.Row;

        var entry = new FontPicker {
            SelectedFontAlias    = row.OriginalValue as string ?? FontCatalog.DefaultFontAlias,
            FontColor = Colors.Black,
            Margin = new Thickness(5, 0, 5, 0),
        };

        entry.AliasChanged += (s, e) => {
            SetValue(row, e);
        };
        
        entry.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;
        return WrapWithLabel(ctx, AddBorder(entry));
        } catch (Exception ex) {
            Logger.LogError(ex, "Error creating FontAlias Renderer for property {PropertyName}", ctx.Row?.Field?.Meta?.Label);
            return new InvalidRenderer(ex.Message).CreateView(ctx);
        }
    }
}