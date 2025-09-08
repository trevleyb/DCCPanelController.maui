using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class BlockRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Block;
    public object CreateView(PropertyContext ctx) {

        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: Block Renderer").CreateView(ctx);

        var blocks = entity.Parent?.Blocks.ToList() ?? [];
        if (blocks.Count == 0) return new InvalidRenderer("No Available Blocks").CreateView(ctx);
        
        var row = ctx.Row;
        var picker = new Picker {
            // BUG: If you have title, it doesn't work.
            // Title = (row.HasMixedValues ? "— mixed —" : null) ?? row.Field.Meta.Label,
            WidthRequest = GetFieldWidth(row.Field.Meta.Width),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
        };
        foreach (var i in blocks) picker.Items.Add(i.DisplayFormat);
        if (row.OriginalValue is string s && picker.Items.Contains(s)) picker.SelectedItem = SelectedIndex(s, blocks);
        picker.SelectedIndexChanged += (s2, e2) => SetValue(row, SelectedValue(picker.SelectedItem, blocks));
        picker.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(picker));
    }

    private int SelectedIndex(string value, List<Block> blocks) => blocks.FindIndex(i => i.Name == value);
    private string SelectedValue(object value, List<Block> blocks) => value is string selectedValue ? blocks.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "" : "";

}