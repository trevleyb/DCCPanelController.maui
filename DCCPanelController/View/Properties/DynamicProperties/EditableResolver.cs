using System.Collections;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties;

public interface IEditorKindResolver {
    string Resolve(EditableField field);
}

public sealed class EditableExtractorResolver : IEditorKindResolver {
    public string Resolve(EditableField field) {
        var meta = field.Meta;
        var t = EditorKinds.UnwrapNullable(field.Accessor.PropertyType);

        // 1) explicit override
        // -----------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(meta.EditorKind)) return meta.EditorKind!;

        // 2) hints via Params
        // -----------------------------------------------------------------------------
        if (TryFromParams(meta, out var byParam)) return byParam;

        // 3) name conventions
        // -----------------------------------------------------------------------------
        var name = field.Prop.Name;
        if (name.Contains("ID", StringComparison.Ordinal) && t == typeof(string)) return EditorKinds.UniqueID;
        if (name.Contains("Choice", StringComparison.Ordinal) && t == typeof(string)) return EditorKinds.Choice;
        if (name.Contains("Color", StringComparison.Ordinal) && t == typeof(Color)) return EditorKinds.Color;
        if (name.Contains("Occupancy", StringComparison.Ordinal)) return EditorKinds.Block;
        if (name.EndsWith("Url", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Url;
        if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Password;
        if (name.EndsWith("Width", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Int;
        if (name.EndsWith("Height", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Int;
        if (name.Contains("Opacity", StringComparison.OrdinalIgnoreCase) && t == typeof(double)) return EditorKinds.Opacity;
        if (name.Contains("Image", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Image;

        if (name.EndsWith("Block", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Block;
        if (name.EndsWith("Route", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Route;
        if (name.EndsWith("Light", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Light;
        if (name.EndsWith("Turnout", StringComparison.Ordinal) || t == typeof(Uri)) return EditorKinds.Turnout;

        
        // 4) type mapping
        // -----------------------------------------------------------------------------
        if (t == typeof(bool)) return EditorKinds.Toggle;
        if (t == typeof(ButtonStateEnum)) return EditorKinds.ButtonState;
        if (t == typeof(TurnoutStateEnum)) return EditorKinds.TurnoutState;
        if (t == typeof(ButtonActions)) return EditorKinds.ButtonActions;
        if (t == typeof(TurnoutActions)) return EditorKinds.TurnoutActions;

        if (t.IsEnum) {
            return Enum.GetNames(t).Length > 4 ? EditorKinds.EnumChoice : EditorKinds.EnumRadio;
        }
        
        if (t == typeof(int) || t == typeof(long)) return EditorKinds.Int;
        if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return EditorKinds.Number;
        if (t == typeof(string)) {
            var format = meta.GetParameters<string>("format", null);
            if (string.Equals(format, "multiline", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Multiline;
            if (string.Equals(format, "password", StringComparison.OrdinalIgnoreCase)) return EditorKinds.Password;
            if (meta.Parameters.ContainsKey("choices")) return EditorKinds.Choice;
            return EditorKinds.Text;
        }
        if (EditorKinds.IsColorType(t)) return EditorKinds.Color;
        if (t == typeof(DateTime) || t.FullName == "System.DateOnly") return EditorKinds.Date;
        if (t == typeof(TimeSpan)) return EditorKinds.TimeSpan;
        if (t == typeof(TimeOnly)) return EditorKinds.TimeSpan;
        if (typeof(IEnumerable).IsAssignableFrom(t) && t != typeof(string)) {
            if (meta.Parameters.ContainsKey("choices")) return EditorKinds.MultiSelect;
        }

        // 5) fallback
        Console.WriteLine($"No editor kind found for {t.Name}");
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
}