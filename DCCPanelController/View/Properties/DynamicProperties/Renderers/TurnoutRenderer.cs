using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class TurnoutRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Turnout;
    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Turnout: Turnout Renderer").CreateView(ctx);

        var turnouts = entity.Parent?.Turnouts.ToList() ?? [];
        if (turnouts.Count == 0) return new InvalidRenderer("No Available Turnouts").CreateView(ctx);
        
        var row = ctx.Row;
        var picker = new Picker {
            WidthRequest = GetFieldWidth(row.Field.Meta.Width),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
        };
        foreach (var i in turnouts) picker.Items.Add(i.DisplayFormat);
        if (row.OriginalValue is string s && picker.Items.Contains(s)) picker.SelectedItem = SelectedIndex(s, turnouts);
        picker.SelectedIndexChanged += (s2, e2) => SetValue(row, SelectedValue(picker.SelectedItem, turnouts));
        picker.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return WrapWithLabel(ctx, AddBorder(picker));
    }

    private int SelectedIndex(string value, List<Turnout> turnouts) {
        return turnouts.FindIndex(i => i.Name == value);
    }

    private string SelectedValue(object value, List<Turnout> turnouts) {
        if (value is string {} selectedValue) return turnouts.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "";
        return "";
    }
}