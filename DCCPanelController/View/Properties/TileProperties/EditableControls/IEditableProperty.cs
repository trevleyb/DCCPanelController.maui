using System.Reflection;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public interface IEditableProperty {
    string Label { get; }
    string Description { get; }
    string Group { get; }
    int Order { get; }
    object? Value { get; set; }
    bool IsModified { get; set; } 
    IView? CreateView(object owner, PropertyInfo info);
}