using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public interface IEditableProperty { 
    string Label { get; }       
    string Description { get; } 
    string Group { get; }       
    int Order { get; }  
    IView? CreateView(object owner, PropertyInfo info);
}