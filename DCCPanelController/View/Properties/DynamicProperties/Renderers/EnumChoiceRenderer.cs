using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class EnumChoiceRenderer : IPropertyRenderer {
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

        var picker = new Picker { Title = row.HasMixedValues ? "— mixed —" : null };
        foreach (var it in items) picker.Items.Add(it.Text);

        // Initial selection (only if not mixed and value present)
        if (!row.HasMixedValues && row.OriginalValue is not null) {
            var idx = items.FindIndex(it => Equals(it.Value, row.OriginalValue));
            if (idx >= 0) picker.SelectedIndex = idx;
        }

        picker.SelectedIndexChanged += (s, e) => {
            if (picker.SelectedIndex < 0) return;
            var selected = items[picker.SelectedIndex].Value;
            RenderBinding.SetValue(row, selected);
        };

        picker.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);

        // wrap with your standard label/description/error grid
        return PropertyRenderers.WrapWithLabel(row, picker);
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