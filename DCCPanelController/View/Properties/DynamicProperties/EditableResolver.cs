using System.Collections;

namespace DCCPanelController.View.Properties.DynamicProperties;

public interface IEditorKindResolver {
    string Resolve(EditableField field);
}

public sealed class EditableExtractorResolver : IEditorKindResolver {
    public string Resolve(EditableField field) {
        var meta = field.Meta;
        var t = UnwrapNullable(field.Accessor.PropertyType);

        // 1) explicit override
        if (!string.IsNullOrWhiteSpace(meta.EditorKind)) return meta.EditorKind!;

        // 2) hints via Params
        if (TryFromParams(meta, out var byParam)) return byParam;

        // 3) name conventions
        var name = field.Prop.Name;
        if (name.EndsWith("Color", StringComparison.Ordinal)) return "color";
        if (name.EndsWith("Url", StringComparison.Ordinal) || t == typeof(Uri)) return "url";
        if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return "password";

        // 4) type mapping
        if (t == typeof(bool)) return "toggle";
        if (t.IsEnum) return "choice";
        if (t == typeof(int) || t == typeof(long)) return "int";
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return "number";
        if (t == typeof(string)) {
            var format = meta.GetParameters<string>("format", null);
            if (string.Equals(format, "multiline", StringComparison.OrdinalIgnoreCase)) return "multiline";
            if (string.Equals(format, "password", StringComparison.OrdinalIgnoreCase)) return "password";
            if (string.Equals(format, "color", StringComparison.OrdinalIgnoreCase)) return "color";
            if (meta.Parameters.ContainsKey("choices")) return "choice";
            return "text";
        }
        if (IsColorType(t)) return "color";
        if (t == typeof(DateTime) || t.FullName == "System.DateOnly") return "date";
        if (t == typeof(TimeSpan)) return "timespan";
        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
            if (meta.Parameters.ContainsKey("choices")) return "multiselect";
        }

        // 5) fallback
        return "text";
    }

    private static bool TryFromParams(EditableAttribute meta, out string kind) {
        var fmt = meta.GetParameters<string>("format", null);
        if (!string.IsNullOrWhiteSpace(fmt)) {
            switch (fmt!.ToLowerInvariant()) {
            case "multiline":
                kind = "multiline";
                return true;

            case "password":
                kind = "password";
                return true;

            case "slider":
                kind = "slider";
                return true;

            case "color":
                kind = "color";
                return true;
            }
        }

        var paramKind = meta.GetParameters<string>("editorKind", null);
        if (!string.IsNullOrWhiteSpace(paramKind)) {
            kind = paramKind!;
            return true;
        }

        kind = default!;
        return false;
    }

    private static Type UnwrapNullable(Type t) => Nullable.GetUnderlyingType(t) ?? t;
    private static bool IsColorType(Type t) => t.FullName == "Microsoft.Maui.Graphics.Color" || t.Name.Equals("Color", StringComparison.Ordinal);
}