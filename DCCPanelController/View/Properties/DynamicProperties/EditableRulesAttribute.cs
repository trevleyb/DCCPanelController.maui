namespace DCCPanelController.View.Properties.DynamicProperties;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EditableRulesAttribute : Attribute {
    public string? IsEnabledWhen { get; init; }
    public bool RespectBaseline { get; init; } = true;
}