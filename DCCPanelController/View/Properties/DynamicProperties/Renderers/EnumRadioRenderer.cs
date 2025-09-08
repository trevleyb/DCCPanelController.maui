using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class EnumRadioRenderer : BaseRenderer,IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) {
        var t = ctx.Row.Field.Accessor.PropertyType;
        var u = Nullable.GetUnderlyingType(t) ?? t;
        return u.IsEnum && ctx.EditorKind == EditorKinds.EnumRadio;
    }

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var propType = row.Field.Accessor.PropertyType;
        var enumType = Nullable.GetUnderlyingType(propType) ?? propType;
        var allowNull = Nullable.GetUnderlyingType(propType) != null;

        var items = BuildEnumItems(enumType, allowNull);
        var groupName = $"{row.Field.DeclaringType.Name}.{row.Field.Prop.Name}.{Guid.NewGuid():N}";
        var stack = new HorizontalStackLayout() { Spacing = 4 };

        // Create a radio for each item
        foreach (var it in items) {
            var rb = new RadioButton {
                Content = it.Text,
                GroupName = groupName,
                IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode)
            };

            // Initial state (if not mixed)
            if (!row.HasMixedValues && row.OriginalValue is not null)
                rb.IsChecked = Equals(it.Value, row.OriginalValue);
            else
                rb.IsChecked = false; // nothing selected when mixed until the user chooses

            rb.CheckedChanged += (_, e) => {
                if (e.Value == true) {
                    SetValue(row, it.Value);
                }
            };

            stack.Add(rb);
        }

        stack.HorizontalOptions = LayoutOptions.Fill;
        var scroll = new ScrollView {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Fill,
            Content = stack,
        };

        // Wrap with your standard label/description/error grid
        return WrapWithLabel(ctx, scroll);
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