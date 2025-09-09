using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyContext {
    public AppMode Mode { get; }
    public PropertyRow Row { get; }
    public CultureInfo Culture { get; }
    public string EditorKind { get; internal set; } = "text"; // resolved kind, set by FormContext
    public IReadOnlyDictionary<string, object> Params => Row.Field.Meta.Parameters;
    public IReadOnlyList<object> Owners { get; }
    
    public PropertyContext(string kind, PropertyRow row, AppMode mode, IReadOnlyList<object>? owners = null, CultureInfo? culture = null) {
        Row = row;
        Mode = mode;
        EditorKind = kind;
        Owners = owners ?? [];
        Culture = culture ?? CultureInfo.InvariantCulture;
    }
    
    public IEnumerable<T> OwnersAs<T>() where T : class => Owners.OfType<T>();
    public T? FirstOwnerAs<T>() where T : class => Owners.OfType<T>().FirstOrDefault();
}

[DebuggerDisplay("{Field?.Meta?.Label} Touched: {IsTouched}")]
public sealed class PropertyRow {
    
    public event EventHandler<object?>? CurrentChanged; 
    
    public EditableField Field { get; }
    public object? OriginalValue { get; set; }
    public bool HasMixedValues { get; set; }
    public bool IsTouched { get; set; }
    public List<ValidationIssue> Issues { get; } = new();
    public PropertyRow(EditableField field) => Field = field;

    public object? CurrentValue {
        get => field;
        set {
            if (field != value) CurrentChanged?.Invoke(this, value);
            field = value;
        }
    }

}


public sealed class PropertyGroup {
    public string Name { get; }
    public int Order { get; }           // used to sort groups, based on first field's Order
    public List<PropertyRow> Rows { get; } = new();
    public PropertyGroup(string name, int order) { Name = name; Order = order; }
}