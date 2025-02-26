using System.Reflection;

namespace DCCPanelController.View.EditProperties.Base;

public static class EditableCollector {

    public static Dictionary<string, List<EditableDetails>> GetEditableProperties(object obj) {
        ArgumentNullException.ThrowIfNull(obj);
        var editableProperties = new List<EditableDetails>();
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        List<string> groups = new();

        for (var i = 0; i < properties.Length; i++) {
            var property = properties[i];
            var attribute = property.GetCustomAttribute<EditableAttribute>();
            if (attribute != null) {
                if (groups.Contains(attribute.Group) == false) groups.Add(attribute.Group);
                editableProperties.Add(new EditableDetails {
                    EditableAttribute = attribute,
                    Info = property,
                    Owner = obj,
                    Type = property.PropertyType,
                    Order = i
                });
            }
        }

        // Sort based on the order that the items were found in the underlying class. 
        // -----------------------------------------------------------------------------------------------------------
        var sorted = new Dictionary<string, List<EditableDetails>>();
        sorted.Add("", GetEditablePropertiesByGroup(""));
        foreach (var group in groups.Where(grp => grp != "")) {
            sorted.Add(group, GetEditablePropertiesByGroup(group));
        }

        return sorted;

        List<EditableDetails> GetEditablePropertiesByGroup(string group) {
            return editableProperties.Where(p => p.EditableAttribute.Group == group).ToList();
        }
    }
}