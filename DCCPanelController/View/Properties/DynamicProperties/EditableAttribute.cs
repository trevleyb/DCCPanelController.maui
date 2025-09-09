using System.Runtime.CompilerServices;
using DCCPanelController.View.Actions;

namespace DCCPanelController.View.Properties.DynamicProperties;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class EditableAttribute : Attribute {
    public EditableAttribute(string label) => Label = label;
    public EditableAttribute(string label, string description) {
        Label = label;
        Description = description;
    }
    public EditableAttribute(string label, string description, int order, string group) {
        Label = label;
        Description = description;
        Order = order;
        Group = group;  
    }
    public EditableAttribute(string label, string description, int order) {
        Label = label;
        Description = description;
        Order = order;
    }
    public EditableAttribute(string label, string description, string group) {
        Label = label;
        Description = description;
        Group = group;  
    }

    
    public string Label { get; }
    public string Description { get; init; } = string.Empty;
    public string Group { get; init; } = "General";
    public int Order { get; init; } = 0;
    public int Width { get; init; } = 0;
    public bool IsReadOnlyInRunMode { get; init; } = false;
    public string EditorKind { get; init; } = string.Empty; // optional override; resolver infers when null/empty

    public string[] Choices { get; init; } = [];
    public bool IsRequired { get; init; } = false;
    public double Min { get; init; } = 0;                // numeric editors (double/decimal)
    public double Max { get; init; } = 99;
    public double Step { get; init; } = 1;

    public ActionsContext ActionsContext { get; set; } = ActionsContext.Unknown;
    
    public Dictionary<string, object> Parameters { get; } = new();

    public T? GetParameters<T>(string key, T? fallback = default) {
        if (Parameters.TryGetValue(key, out var obj) && obj is T t) return t;
        return fallback;
    }
}