namespace DCCPanelController.View.Properties.DynamicProperties;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class EditableAttribute : Attribute {
    public EditableAttribute(string label) => Label = label;

    public string Label { get; }
    public string Description { get; init; } = string.Empty;
    public string Group { get; init; } = "General";
    public string EditorKind { get; init; } = string.Empty; // optional override; resolver infers when null/empty
    public bool IsReadOnlyInRunMode { get; init; } = false;
    public int Order { get; init; } = 0;
    public int Width { get; init; } = 0;

    public string[] Choices { get; init; } = [];
    public bool IsRequired { get; init; } = false;
    public double Min { get; init; } = int.MinValue;                // numeric editors (double/decimal)
    public double Max { get; init; } = int.MaxValue;
    public double Step { get; init; } = 1;
    
    public Dictionary<string, object> Parameters { get; } = new();

    public T? GetParameters<T>(string key, T? fallback = default) {
        if (Parameters.TryGetValue(key, out var obj) && obj is T t) return t;
        return fallback;
    }
}