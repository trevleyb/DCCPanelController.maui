using System.Diagnostics;
using System.Globalization;

namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyContext {
    public PropertyRow Row { get; }
    public AppMode Mode { get; }
    public CultureInfo Culture { get; }
    public string EditorKind { get; internal set; } = "text"; // resolved kind, set by FormContext
    public System.Collections.Generic.IReadOnlyDictionary<string, object> Params => Row.Field.Meta.Parameters;

    public PropertyContext(PropertyRow row, AppMode mode, CultureInfo? culture = null) {
        Row = row;
        Mode = mode;
        Culture = culture ?? CultureInfo.InvariantCulture;
    }
}

[DebuggerDisplay("{Field?.Meta?.Label} Touched: {IsTouched}")]
public sealed class PropertyRow {
    public EditableField Field { get; }
    public object? OriginalValue { get; set; }
    public object? CurrentValue { get; set; }
    public bool HasMixedValues { get; set; }
    public bool IsTouched { get; set; }
    public List<ValidationIssue> Issues { get; } = new();
    public PropertyRow(EditableField field) => Field = field;
}


public sealed class PropertyGroup {
    public string Name { get; }
    public int Order { get; }           // used to sort groups, based on first field's Order
    public List<PropertyRow> Rows { get; } = new();
    public PropertyGroup(string name, int order) { Name = name; Order = order; }
}