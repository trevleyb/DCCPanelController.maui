using System.Collections;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Properties.DynamicProperties;

public interface IEditorKindResolver {
    string Resolve(EditableField field);
}

public sealed class EditableExtractorResolver : IEditorKindResolver {
    public string Resolve(EditableField field) {
        var meta = field.Meta;
        var t = UnwrapNullable(field.Accessor.PropertyType);

        // 1) explicit override
        // -----------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(meta.EditorKind)) return meta.EditorKind!;

        // 2) hints via Params
        // -----------------------------------------------------------------------------
        if (TryFromParams(meta, out var byParam)) return byParam;

        // 3) name conventions
        // -----------------------------------------------------------------------------
        var name = field.Prop.Name;
        if (name.Contains("Color", StringComparison.Ordinal)) return EditorKinds.Color;
        if (name.Contains("Occupancy", StringComparison.Ordinal)) return EditorKinds.Block;
        if (name.EndsWith("Block", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Block;
        if (name.EndsWith("Url", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Url;
        if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Password;
        if (name.EndsWith("Width", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Int;
        if (name.EndsWith("Height", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Int;
        if (name.Contains("Opacity", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Opacity;
        if (name.Contains("Image", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Image;
        
        // 4) type mapping
        // -----------------------------------------------------------------------------
        if (t == typeof(bool)) return EditorKinds.Toggle;
        if (t == typeof(ButtonStateEnum)) return EditorKinds.Button;
        
        if (t.IsEnum) return EditorKinds.EnumRadio;
        if (t == typeof(int) || t == typeof(long)) return EditorKinds.Int;
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return EditorKinds.Number;
        if (t == typeof(string)) {
            var format = meta.GetParameters<string>("format", null);
            if (string.Equals(format, "multiline", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Multiline;
            if (string.Equals(format, "password", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Password;
            if (meta.Parameters.ContainsKey("choices")) return EditorKinds.Choice;
            return EditorKinds.Text;
        }
        if (IsColorType(t)) return EditorKinds.Color;
        if (t == typeof(DateTime) || t.FullName == "System.DateOnly") return EditorKinds.Date;
        if (t == typeof(TimeSpan)) return EditorKinds.TimeSpan;
        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
            if (meta.Parameters.ContainsKey("choices")) return EditorKinds.MultiSelect;
        }

        // 5) fallback
        return EditorKinds.Text;
    }

    private static bool TryFromParams(EditableAttribute meta, out string kind) {
        var fmt = meta.GetParameters<string>("format", null);
        if (!string.IsNullOrWhiteSpace(fmt)) {
            switch (fmt!.ToLowerInvariant()) {
            case "multiline":
                kind = EditorKinds.Multiline;
                return true;

            case "password":
                kind = EditorKinds.Password;
                return true;

            case "color":
                kind = EditorKinds.Color;
                return true;
            }
        }

        var paramKind = meta.GetParameters<string>("editorKind", null);
        if (!string.IsNullOrWhiteSpace(paramKind)) {
            kind = paramKind!;
            return true;
        }

        kind = EditorKinds.Text;
        return false;
    }

    private static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;
    private static bool IsColorType(Type t) => t.FullName == "Microsoft.Maui.Graphics.Color" || t.Name.Equals("Color", StringComparison.Ordinal);
}