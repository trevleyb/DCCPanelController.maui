using System.Reflection;

namespace DCCPanelController.Helpers.EditableProperties;

public class EditablePropertyCollector {
    public static Dictionary<string, List<EditablePropertyDetails>> GetEditableProperties(object obj) {
        ArgumentNullException.ThrowIfNull(obj);
        var editableProperties = new List<EditablePropertyDetails>();
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < properties.Length; i++) {
            var property = properties[i];
            var attribute = property.GetCustomAttribute<EditableProperty>();
            if (attribute != null) {
                editableProperties.Add(new EditablePropertyDetails {
                    Attribute = attribute,
                    Info = property,
                    Owner = obj,
                    Type = property.PropertyType,
                    Order = i
                });
            }
        }

        // Sort by Group, SortOrder, then by ReadOrder and create a Dictionary
        // -----------------------------------------------------------------------------------------------------------
        var sorted = new Dictionary<string, List<EditablePropertyDetails>>();
        foreach (var editableProperty in editableProperties.OrderBy(p => p.Attribute.Group).ThenBy(p => p.Attribute.Order).ThenBy(p => p.Order).ToList()) {
            if (!sorted.TryGetValue(editableProperty.Attribute.Group, out var value)) {
                value = [];
                sorted.Add(editableProperty.Attribute.Group, value);
            }

            value.Add(editableProperty);
        }

        return sorted;
    }
}