using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DCCPanelController.Models.DataModel.Helpers;

public static class EditableExtractor {
    public static List<(PropertyInfo Property, EditableAttribute Metadata)> GetEditableProperties(object entity) {
        var properties = entity.GetType()
                               .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                               .Where(p => p.GetCustomAttribute<EditableAttribute>() != null);

        return properties
              .Select(p => (p, p.GetCustomAttribute<EditableAttribute>()!))
              .OrderBy(p => p.Item2.Group) // Group by Group Name
              .ThenBy(p => p.Item2.Order)  // Then sort by Order
              .ToList();
    }
}