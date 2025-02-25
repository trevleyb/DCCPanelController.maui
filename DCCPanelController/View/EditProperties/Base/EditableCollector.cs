using System.Reflection;

namespace DCCPanelController.View.EditProperties.Base;

public static class EditableCollector {

    public static Dictionary<string, List<EditableDetails>> GetEditableProperties(object obj) {
        ArgumentNullException.ThrowIfNull(obj);
        var editableProperties = new List<EditableDetails>();
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (var i = 0; i < properties.Length; i++) {
            var property = properties[i];
            var attribute = property.GetCustomAttribute<EditableAttribute>();
            if (attribute != null) {
                editableProperties.Add(new EditableDetails {
                    EditableAttribute = attribute,
                    Info = property,
                    Owner = obj,
                    Type = property.PropertyType,
                    Order = i
                });
            }
        }

        // Sort by Group, SortOrder, then by ReadOrder and create a Dictionary
        // -----------------------------------------------------------------------------------------------------------
        var sorted = new Dictionary<string, List<EditableDetails>>();
        foreach (var editableProperty in editableProperties.OrderBy(p => p.EditableAttribute.Group).ThenBy(p => p.EditableAttribute.Order).ThenBy(p => p.Order).ToList()) {
            if (!sorted.TryGetValue(editableProperty.EditableAttribute.Group, out var value)) {
                value = [];
                sorted.Add(editableProperty.EditableAttribute.Group, value);
            }
            value.Add(editableProperty);
        }
        return sorted;
    }
}