namespace DCCPanelController.Models.DataModel.Helpers;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EditableAttribute : Attribute
{
    public string Label { get; }                    // Label/Prompt for the property
    public string Group { get; }                    // Group to which this property belongs
    public EditableType Type { get; }               // What type of property is this?
    public int Order { get; } = 0;                  // Order within the group

    public EditableAttribute(string label, EditableType type, string group) {
        Label = label;
        Type = type;
        Order = 0;
        Group = group;
    }

    public EditableAttribute(string label, EditableType type, int order = 0, string group = "") {
        Label = label;
        Type = type;
        Order = order;
        Group = group;
    }
}

public enum EditableType {
    String, 
    Integer,
    Double,
    Switch,
    Color,
    Id,
    ButtonActions,
    TurnoutActions,
    TrackType,
    TrackTerminator, 
    TrackAttributes,
    AspectRatio,
    Image,
    Opacity,
    Alignment
}