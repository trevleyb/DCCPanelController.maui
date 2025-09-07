namespace DCCPanelController.View.Properties.DynamicProperties;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class EditableAttribute : Attribute {
    public string Label { get; }
    public string? Description { get; init; }
    public string Group { get; init; } = "General";
    public int Order { get; init; } = 0;
    public int Width { get; init; } = 150;
    public string? EditorKind { get; init; } = null; // optional override; resolver infers when null/empty
    public bool IsReadOnlyInRunMode { get; init; } = false;
    public EditableAttribute(string label) => Label = label;

    public string[]? Choices { get; init; }           // for choice/enum editors
    public bool IsRequired { get; init; } = false;
    public int? MinInt { get; init; }
    public int? MaxInt { get; init; }
    public int? StepInt { get; init; }
    public double? Min { get; init; }                 // numeric editors (double/decimal)
    public double? Max { get; init; }
    public double? Step { get; init; }
    
    public Dictionary<string, object> Parameters { get; } = new();

    public T? GetParameters<T>(string key, T? fallback = default) {
        if (Parameters.TryGetValue(key, out var obj) && obj is T t) return t;
        return fallback;
    }
    
    internal void MaterializeParameters() {
        Parameters["required"] = IsRequired;
        if (Choices is { Length: > 0 }) Parameters["choices"] = Choices;
        if (MinInt.HasValue) Parameters["min"] = MinInt.Value;
        if (MaxInt.HasValue) Parameters["max"] = MaxInt.Value;
        if (StepInt.HasValue) Parameters["step"] = StepInt.Value;

        if (Min.HasValue) Parameters["min"] = Min.Value;
        if (Max.HasValue) Parameters["max"] = Max.Value;
        if (Step.HasValue) Parameters["step"] = Step.Value;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public sealed class EditableParamAttribute : Attribute {
    public string Key { get; }
    public object? BoxedValue { get; } 

    public EditableParamAttribute(string key, string value)    { Key = key; BoxedValue = value; }
    public EditableParamAttribute(string key, int value)       { Key = key; BoxedValue = value; }
    public EditableParamAttribute(string key, double value)    { Key = key; BoxedValue = value; }
    public EditableParamAttribute(string key, bool value)      { Key = key; BoxedValue = value; }
    public EditableParamAttribute(string key, Type value)      { Key = key; BoxedValue = value; }
    public EditableParamAttribute(string key, string[] values) { Key = key; BoxedValue = values; }
}
