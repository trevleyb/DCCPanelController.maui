using System.Reflection;
using DCCPanelController.View.Properties.TileProperties.EditableControls;

namespace DCCPanelController.View.Properties.TileProperties.Attributes;

public static class EditableExtractor {
    public static List<(PropertyInfo Property, IEditableProperty Metadata)> GetEditableProperties(object entity) {
        var properties = entity.GetType()
                               .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                               .Where(p => p.GetCustomAttribute<EditableProperty>() != null);

        var props = properties.Select(p => (
                                          Property: p,
                                          Metadata: (IEditableProperty)p.GetCustomAttribute<EditableProperty>()!
                                      ))
                              .OrderBy(p => p.Metadata.Order)
                              .ThenBy(p => p.Metadata.Group)
                              .ToList();

        return props;
    }
}