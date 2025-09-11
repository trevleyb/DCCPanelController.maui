using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class EnumChoiceRenderer : BaseRenderer,IPropertyRenderer {
    
    protected override int FieldWidth => 250;
    public bool CanRender(PropertyContext ctx) {
        var t = ctx.Row.Field.Accessor.PropertyType;
        var u = Nullable.GetUnderlyingType(t) ?? t;
        return u.IsEnum && (ctx.EditorKind == EditorKinds.EnumChoice || ctx.EditorKind == EditorKinds.Choice);
    }

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var propType = row.Field.Accessor.PropertyType;
        var enumType = Nullable.GetUnderlyingType(propType) ?? propType;
        var allowNull = Nullable.GetUnderlyingType(propType) != null;

        // Build items (DisplayAttribute-friendly)
        var items = BuildEnumItems(enumType, allowNull);

        var picker = new Picker {
            Title = (row.HasMixedValues ? "— mixed —" : null) ?? string.Empty,
            FontSize = FieldFontSize,
            WidthRequest = GetFieldWidth(ctx),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(5, 0, 5, 0)
        };
        foreach (var it in items) picker.Items.Add(it.Text);

        // Initial selection (only if not mixed and value present)
        if (!row.HasMixedValues && row.OriginalValue is not null) {
            var idx = items.FindIndex(it => Equals(it.Value, row.OriginalValue));
            if (idx >= 0) picker.SelectedIndex = idx;
        }

        picker.SelectedIndexChanged += (s, e) => {
            if (picker.SelectedIndex < 0) return;
            var selected = items[picker.SelectedIndex].Value;
            SetValue(row, selected);
        };

        picker.IsEnabled = !(row.Field.Meta.IsReadOnlyInRunMode);

        // wrap with your standard label/description/error grid
        return WrapWithLabel(ctx, AddBorder(picker));
    }

    private static List<(string Text, object? Value)> BuildEnumItems(Type enumType, bool includeNone) {
        var list = new List<(string, object?)>();
        if (includeNone) list.Add(("(None)", null));

        foreach (var val in Enum.GetValues(enumType)) {
            var name = Enum.GetName(enumType, val)!;
            var fi = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            var display = fi?.GetCustomAttribute<DisplayAttribute>()?.GetName();
            list.Add((display ?? name, val));
        }
        return list;
    }
}