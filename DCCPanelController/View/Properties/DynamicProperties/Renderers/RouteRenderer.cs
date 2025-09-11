using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class RouteRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 200;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Route;
    public object CreateView(PropertyContext ctx) {
        var entity = ctx.FirstOwnerAs<IEntity>();
        if (entity == null) return new InvalidRenderer("Cant find owning Route: Route Renderer").CreateView(ctx);

        var routes = entity.Parent?.Routes.ToList() ?? [];
        if (routes.Count == 0) return new InvalidRenderer("No Available Routes").CreateView(ctx);
        
        var row = ctx.Row;
        var picker = new Picker {
            WidthRequest = GetFieldWidth(ctx),
            FontSize = FieldFontSize,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 0, 0, 0)
        };
        foreach (var i in routes) picker.Items.Add(i.DisplayFormat);
        if (row.OriginalValue is string s) picker.SelectedIndex = SelectedIndex(s, routes);
        picker.SelectedIndexChanged += (s2, e2) => SetValue(row, SelectedValue(picker.SelectedItem, routes));
        picker.IsEnabled = !(row.Field.Meta.IsReadOnlyInRunMode);

        var wrapped = WrapPicker(ctx, picker, GetFieldWidth(ctx));
        return WrapWithLabel(ctx, AddBorder(wrapped));

    }

    private int SelectedIndex(string value, List<Route> routes) {
        return routes.FindIndex(i => i.Name == value);
    }

    private string SelectedValue(object value, List<Route> routes) {
        if (value is string {} selectedValue) return routes.Find(i => i.DisplayFormat == selectedValue)?.Name ?? "";
        return "";
    }
    
}
