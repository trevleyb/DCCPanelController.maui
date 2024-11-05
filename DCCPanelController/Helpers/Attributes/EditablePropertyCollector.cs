using System.Reflection;

namespace DCCPanelController.Helpers.Attributes;

public class EditablePropertyCollector {

    public static List<EditablePropertyDetails> GetEditableProperties(object obj) {
        ArgumentNullException.ThrowIfNull(obj);
        var editableProperties = new List<EditablePropertyDetails>();

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < properties.Length; i++) {
            var property = properties[i];
            var attribute = property.GetCustomAttribute<EditablePropertyAttribute>();
            if (attribute != null) {
                editableProperties.Add(new EditablePropertyDetails() {
                    Attribute = attribute,
                    Info = property,
                    Owner = obj,
                    Type = property.PropertyType,
                    Order = i
                });
            }
        }

        // Sort by Group, SortOrder, then by ReadOrder
        return editableProperties
            .OrderBy(p => p.Attribute.Group)
            .ThenBy(p => p.Attribute.Order)
            .ThenBy(p => p.Order)
            .ToList();        
    }
}
