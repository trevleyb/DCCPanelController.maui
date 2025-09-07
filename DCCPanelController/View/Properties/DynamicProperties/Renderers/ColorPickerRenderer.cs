using DCCPanelController.Helpers;
using DCCPanelController.View.Components; // <-- your ColorPickerButton namespace

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class ColorPickerRenderer : IPropertyRenderer {
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == EditorKinds.Color;
    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;

        // Build your custom control
        var picker = new ColorPickerButton {
            WidthRequest = 150,
            HeightRequest = 30,
            SelectedColor = row.OriginalValue is Color c ? c : null,
            AllowsNoColor = row.Field.Meta.GetParameters("allowsNoColor", true),
            IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode),
        };

        // Default color (optional param)
        if (TryGetParamColor(row.Field.Meta, "defaultColor", out var def)) {
            picker.DefaultColor = def;
        }

        // Seed SelectedColor from value (Color or string)
        if (TryGetRowColor(row, out var start)) {
            picker.SelectedColor = start;
        } else {
            picker.SelectedColor = null; // will render as "Use Default" in your control
        }

        // If mixed, overlay a small "— mixed —" chip until user edits
        Microsoft.Maui.Controls.View visual = picker;
        // if (row.HasMixedValues) {
        //     var overlay = new Label {
        //         Text = "— mixed —",
        //         Opacity = 0.6,
        //         VerticalTextAlignment = TextAlignment.Center,
        //         HorizontalTextAlignment = TextAlignment.Center
        //     };
        //     var grid = new Grid();
        //     grid.Add(picker);
        //     grid.Add(overlay);
        //
        //     // hide overlay once the value changes
        //     void HideOverlay() => overlay.IsVisible = false;
        //     picker.PropertyChanged += (_, e) => {
        //         if (e.PropertyName == nameof(ColorPickerButton.SelectedColor) || e.PropertyName == "SelectedColorProperty")
        //             HideOverlay();
        //     };
        //     visual = grid;
        // }

        // Push changes back into the form row
        picker.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ColorPickerButton.SelectedColor) || e.PropertyName == "SelectedColorProperty") {
                RenderBinding.SetValue(row, picker.SelectedColor);
            }
        };
        return PropertyRenderers.WrapWithLabel(row, visual);
    }

    // --- helpers ---

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