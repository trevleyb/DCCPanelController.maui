using System.Reflection;

namespace DCCPanelController.View.DynamicProperties.EditableControls;

public interface IEditableProperty { 
    string Label { get; }       
    string Description { get; } 
    string Group { get; }       
    int Order { get; }  
    IView? CreateView(object owner, PropertyInfo info, Action<string>? propertyModified = null);
}