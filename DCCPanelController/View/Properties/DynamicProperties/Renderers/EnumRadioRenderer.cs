using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

internal sealed class EnumRadioRenderer : BaseRenderer, IPropertyRenderer {
    protected override int FieldHeight => -1;
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

        //var stack = new HorizontalStackLayout() { Spacing = 4 };
        var stack = new FlexLayout {
            AlignContent = FlexAlignContent.Start,
            AlignItems = FlexAlignItems.Start,
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
        };

        // Create a radio for each item
        foreach (var it in items) {
            var rb = new RadioButton {
                Content = it.Text,
                FontSize = FieldFontSize,
                GroupName = groupName,
                Margin = new Thickness(0, 0, 0, 0),
                IsEnabled = !row.Field.Meta.IsReadOnlyInRunMode,
                IsChecked = row is { HasMixedValues: false, OriginalValue: { } } && Equals(it.Value, row.OriginalValue), // nothing selected when mixed until the user chooses
            };

            rb.CheckedChanged += (_, e) => {
                if (e.Value) SetValue(row, it.Value);
            };
            stack.Add(rb);
        }

        stack.HorizontalOptions = LayoutOptions.Fill;
        return WrapWithLabel(ctx, stack);
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