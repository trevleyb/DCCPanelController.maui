using CoreText;
using DCCPanelController.View.Components;
using Microsoft.Maui.Graphics;

// <-- your ColorPickerButton namespace

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

public sealed class ColorPickerRenderer : BaseRenderer,IPropertyRenderer {
    protected override int FieldWidth => 150;
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Color;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var picker = new ColorPickerButton {
            SelectedColor = row.OriginalValue is Color c ? c : null,
            AllowsNoColor = true,
            IsEnabled = !(row.Field.Meta.IsReadOnlyInRunMode),
            IsMultiValue = ctx.Row.HasMixedValues
        };

        // Seed SelectedColor from value (Color or string)
        picker.SelectedColor = TryGetRowColor(row, out var start) ? start : null; // will render as "Use Default" in your control

        Microsoft.Maui.Controls.View visual = picker;

        // Push changes back into the form row
        picker.PropertyChanged += (_, e) => {
            if (e.PropertyName is nameof(ColorPickerButton.SelectedColor) or "SelectedColorProperty") {
                SetValue(row, picker.SelectedColor);
            }
        };
        return WrapWithLabel(ctx, visual);
    }

    // Read a Color from the row's value (supports Color or hex string)
    private static bool TryGetRowColor(PropertyRow row, out Color? result) {
        if (row.OriginalValue is Color c1) {
            result = c1;
            return true;
        }
        if (row.CurrentValue is Color c2) {
            result = c2;
            return true;
        }
        if (row.OriginalValue is string s1 && TryParseColor(s1, out var c3)) {
            result = c3;
            return true;
        }
        if (row.CurrentValue is string s2 && TryParseColor(s2, out var c4)) {
            result = c4;
            return true;
        }
        result = null;
        return false;
    }

    // Read a Color from attribute Params (accepts Color or hex string)
    private static bool TryGetParamColor(EditableAttribute meta, string key, out Color? result) {
        if (meta.Parameters.TryGetValue(key, out var obj)) {
            if (obj is Color c) {
                result = c;
                return true;
            }
            if (obj is string s && TryParseColor(s, out var parsed)) {
                result = parsed;
                return true;
            }
        }
        result = null;
        return false;
    }

    // naive hex parser: "#RGB", "#RRGGBB", or "#AARRGGBB"
    private static bool TryParseColor(string s, out Color? result) {
        try {
            if (string.IsNullOrWhiteSpace(s)) {
                result = null;
                return false;
            }
            s = s.Trim();

            // FromArgb handles #RRGGBB and #AARRGGBB
            if (s.StartsWith("#", StringComparison.Ordinal)) {
                if (s.Length == 4) // #RGB -> expand
                {
                    s = $"#{s[1]}{s[1]}{s[2]}{s[2]}{s[3]}{s[3]}";
                }
                result = Color.FromArgb(s);
                return true;
            }

            // Named fallback: try Colors.<Name> via reflection
            var prop = typeof(Colors).GetProperty(s, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
            if (prop?.GetValue(null) is Color named) {
                result = named;
                return true;
            }
        } catch { /* ignore */
        }
        result = null;
        return false;
    }
}