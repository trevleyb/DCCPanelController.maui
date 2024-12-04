namespace DCCPanelController.Helpers.EditableProperties;

[AttributeUsage(AttributeTargets.Property)]
public abstract class EditableProperty : Attribute {
    public required string Name { get; set; }               // SystemName to show on the Properties Page
    public string Description { get; set; } = string.Empty; // Description to show under/next to the Property
    public string Group { get; set; } = string.Empty;       // Group Identifier. Used to group items in a box
    public int Order { get; set; } = 0;                     // What is the Sort order. If 0, then by order in the class
}