using System.Reflection;
using DCCPanelController.Models.DataModel.Helpers;

namespace DCCPanelController.View.DynamicProperties;

public interface IEditableProperty { 
    IView? CreateView(object owner, PropertyInfo info, EditableAttribute attribute);
}