using System.Diagnostics;
using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyContext {
    public PropertyContext(string kind, PropertyRow row, double width, double height, IReadOnlyList<object>? owners = null, CultureInfo? culture = null) {
        Row = row;
        EditorKind = kind;
        Width = width;
        Height = height;
        Owners = owners ?? [];
        Culture = culture ?? CultureInfo.InvariantCulture;
    }

    public PropertyRow Row { get; }
    public CultureInfo Culture { get; }
    public string EditorKind { get; internal set; } = "text"; // resolved kind, set by FormContext
    public IReadOnlyDictionary<string, object> Params => Row.Field.Meta.Parameters;
    public IReadOnlyList<object> Owners { get; }
    public double Width { get; set; }
    public double Height { get; set; }

    public IEnumerable<T> OwnersAs<T>() where T : class => Owners.OfType<T>();
    public T? FirstOwnerAs<T>() where T : class => Owners.OfType<T>().FirstOrDefault();
}

[DebuggerDisplay("{Field?.Meta?.Label} Touched: {IsTouched}")]
public sealed class PropertyRow {
    public PropertyRow(EditableField field) => Field = field;

    public EditableField Field { get; }
    public object? OriginalValue { get; set; }
    public bool HasMixedValues { get; set; }
    public bool IsTouched { get; set; }
    public List<ValidationIssue> Issues { get; } = new();

    public object? CurrentValue {
        get => field;
        set {
            if (field != value) CurrentChanged?.Invoke(this, value);
            field = value;
        }
    }

    public event EventHandler<object?>? CurrentChanged;
}

public sealed class PropertyGroup {
    public PropertyGroup(string name, int order) {
        Name = name;
        Order = order;
    }

    public string Name { get; }
    public int Order { get; } // used to sort groups, based on first field's Order
    public List<PropertyRow> Rows { get; } = new();
}