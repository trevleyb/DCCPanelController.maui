namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyRow {
    public EditableField Field { get; }
    public object? OriginalValue { get; set; }
    public object? CurrentValue { get; set; }
    public bool HasMixedValues { get; set; }
    public bool IsTouched { get; set; }
    public List<ValidationIssue> Issues { get; } = new();
    public PropertyRow(EditableField field) => Field = field;
}
