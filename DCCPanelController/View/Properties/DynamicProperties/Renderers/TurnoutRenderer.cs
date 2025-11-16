using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class TurnoutRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Turnout;

    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Turnout: Turnout Renderer").CreateView(ctx);

        var turnouts = entity.Parent?.Turnouts.ToList() ?? [];
        if (turnouts.Count == 0) return new InvalidRenderer("No Available Turnouts").CreateView(ctx);

        var row = ctx.Row;
        var picker = new Picker {
            WidthRequest = GetFieldWidth(ctx),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };
        
        picker.ItemsSource = turnouts;
        picker.ItemDisplayBinding = new Binding(nameof(Turnout.DisplayFormat));
        if (row.OriginalValue is string s) {
            var item = turnouts.FirstOrDefault(b => b.SystemId == s) ?? turnouts.FirstOrDefault(b => b.Name == s);
            picker.SelectedItem = item;
        }
        picker.SelectedIndexChanged += (s2, e2) => {
            if (picker.SelectedIndex < turnouts.Count && picker.SelectedIndex >= 0) {
                var item = turnouts[picker.SelectedIndex];
                SetValue(row, item.SystemId);
            }
        };
        picker.IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode;

        
        // foreach (var i in turnouts) picker.Items.Add(i.DisplayFormat);
        // if (row.OriginalValue is string s) picker.SelectedIndex = SelectedIndex(s, turnouts);
        // picker.SelectedIndexChanged += (s2, e2) => SetValue(row, SelectedValue(picker.SelectedItem, turnouts));

        var wrapped = WrapPicker(ctx, picker, GetFieldWidth(ctx));
        return WrapWithLabel(ctx, AddBorder(wrapped));
    }

    private int SelectedIndex(string value, List<Turnout> turnouts) => turnouts.FindIndex(i => i.Name == value);

    private string SelectedValue(object value, List<Turnout> turnouts) {
        if (value is string { } selectedValue) return turnouts.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "";
        return"";
    }
}