using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using DCCPanelController.View.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Helpers;

public static class EditableExtractor {

    public static List<(PropertyInfo Property, IEditableProperty Metadata)> GetEditableProperties(object entity) {
    
        var properties = entity.GetType()
                               .GetProperties(BindingFlags.Public | BindingFlags.Instance )
                               .Where(p => p.GetCustomAttribute<EditableProperty>() != null);

        var props= properties.Select(p => (
                Property: p, 
                Metadata: (IEditableProperty)p.GetCustomAttribute<EditableProperty>()!
            ))
            .OrderBy(p => p.Metadata.Order) 
            .ThenBy(p => p.Metadata.Group)
            .ToList();

        return props;
        
        // var props  = properties
        //       .Select(p => (entity, p))
        //       .OrderBy(p => p.Item .Order) // Then sort by Order
        //       .ThenBy(p => p.Item3.Group)  // Group by Group Name
        //       .ToList();
        // return props;
    }
}