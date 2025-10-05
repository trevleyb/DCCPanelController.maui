using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class BlockRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Block;

    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: Block Renderer").CreateView(ctx);

        var blocks = entity.Parent?.Blocks.ToList() ?? [];
        if (blocks.Count == 0) return new InvalidRenderer("No Available Blocks").CreateView(ctx);

        var row = ctx.Row;
        var picker = new Picker {
            FontSize = FieldFontSize,
            Margin = new Thickness(5, 0, 0, 0),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
        };
        picker.ItemsSource = blocks;
        picker.ItemDisplayBinding = new Binding(nameof(Block.DisplayFormat));
        if (row.OriginalValue is string s) {
            var item = blocks.FirstOrDefault(b => b.Id == s) ?? blocks.FirstOrDefault(b => b.Name == s);
            picker.SelectedItem = item;
        }
        picker.SelectedIndexChanged += (s2, e2) => {
            if (picker.SelectedIndex < blocks.Count && picker.SelectedIndex >= 0) {
                var item = blocks[picker.SelectedIndex];
                SetValue(row, item.Id);
            }
        };
        picker.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;

        var wrapped = WrapPicker(ctx, picker, GetFieldWidth(ctx));
        return WrapWithLabel(ctx, AddBorder(wrapped));
    }

    private int SelectedIndex(string value, List<Block> blocks) => blocks.FindIndex(i => i.Name == value);
    private string SelectedValue(object value, List<Block> blocks) => value is string selectedValue ? blocks.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "" : "";
}