namespace DCCPanelController.View.Properties.DynamicProperties;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class EditableAttribute : Attribute {
    public string Label { get; }
    public string? Description { get; init; }
    public string Group { get; init; } = "General";
    public int Order { get; init; } = 0;
    public string? EditorKind { get; init; } = null; // optional override; resolver infers when null/empty
    public bool IsReadOnlyInRunMode { get; init; } = false;

    /// <summary>Arbitrary parameters for renderer/validation (min, max, step, regex, choices, format, etc.)</summary>
    public Dictionary<string, object> Parameters { get; } = new();

    public EditableAttribute(string label) => Label = label;

    public T? GetParameters<T>(string key, T? fallback = default) {
        if (Parameters.TryGetValue(key, out var obj) && obj is T t) return t;
        return fallback;
    }
}