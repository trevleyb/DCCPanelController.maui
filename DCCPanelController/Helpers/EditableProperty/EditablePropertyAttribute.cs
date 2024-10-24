namespace DCCPanelController.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditablePropertyAttribute : Attribute {

    public string Name { get; set; }            // Name to show on the Properties Page
    public int Order { get; set; }              // What is the Sort order. If 0, then by order in the class

    public string Description { get; set; }     // Description to show under/next to the Property
    public string Group { get; set; }           // Group Identifier. Used to group items in a box
    
    public int MaxLength { get; set; }          // used for Strings only
    public int MinValue { get; set; }           // used for Int (Minimum Value)
    public int MaxValue { get; set; }           // used for Int (Maximum Value)
}