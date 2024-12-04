namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public class EditableIntPropertyAttribute : EditableProperty {
    public int MinValue { get; set; } = 0;   // used for Int (Minimum Value)
    public int MaxValue { get; set; } = 999; // used for Int (Maximum Value)
}