using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class LightRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Light;

    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Object: Light Renderer").CreateView(ctx);

        var lights = entity.Parent?.Lights.ToList() ?? [];
        if (lights.Count == 0) return new InvalidRenderer("No Available lights").CreateView(ctx);

        var row = ctx.Row;
        var picker = new Picker {
            WidthRequest = GetFieldWidth(ctx),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };
        picker.ItemsSource = lights;
        picker.ItemDisplayBinding = new Binding(nameof(Light.DisplayFormat));
        if (row.OriginalValue is string s) {
            var item = lights.FirstOrDefault(b => b.Id == s) ?? lights.FirstOrDefault(b => b.Name == s);
            picker.SelectedItem = item;
        }
        picker.SelectedIndexChanged += (s2, e2) => {
            if (picker.SelectedIndex < lights.Count && picker.SelectedIndex >= 0) {
                var item = lights[picker.SelectedIndex];
                SetValue(row, item.Id);
            }
        };
        picker.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;

        var wrapped = WrapPicker(ctx, picker, GetFieldWidth(ctx));
        return WrapWithLabel(ctx, AddBorder(wrapped));
    }

    private int SelectedIndex(string value, List<Light> lights) => lights.FindIndex(i => i.Name == value);
    private string SelectedValue(object value, List<Light> lights) => value is string selectedValue ? lights.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "" : "";
}