namespace DCCPanelController.Models.DataModel.Helpers;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EditableAttribute : Attribute
{
    public string Label { get; }                    // Label/Prompt for the property
    public string Group { get; }                    // Group to which this property belongs
    public EditableType Type { get; }               // What type of property is this?
    public int Order { get; } = 0;                  // Order within the group
    public string Description { get; set; } = "";   // Description of the property
    public object[] Options { get; set; }           // Options for the property

    public EditableAttribute(string label, EditableType type) : this(label, type, "") { }

    public EditableAttribute(string label, EditableType type, string group, params object[] options) {
        Label = label;
        Type = type;
        Order = 0;
        Group = group;
        Options = options;
    }

    public EditableAttribute(string label, EditableType type, int order, string group, params object[] options) : this(label, type, group, options) {
        Order = order;
    }

    public T? GetOption<T>(int parameter) {
        if (parameter < 0 || parameter >= Options.Length) return default(T);
        return (T)Options[parameter];
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