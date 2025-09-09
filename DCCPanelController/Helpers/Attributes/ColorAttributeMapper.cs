using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Helpers;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Helpers.Attributes;

/// <summary>
///     Attribute to mark fields that should generate copyable properties with UI metadata
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CopyableAttribute : Attribute {
    public CopyableAttribute(string group) {
        Group = group;
    }

    public string Group { get; }
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CategorySortOrder { get; set; } = 100; // Default sort order for categories
    public int ItemSortOrder { get; set; } = 100;     // Default sort order for items within category
}

/// <summary>
///     Represents a grouped collection of PanelColorItems
/// </summary>
public class ColorItemGroup : List<PanelColorItem> {
    public ColorItemGroup(string categoryName, int categorySortOrder, IEnumerable<PanelColorItem> items) : base(items) {
        CategoryName = categoryName;
        CategorySortOrder = categorySortOrder;
    }

    public string CategoryName { get; set; } 
    public int CategorySortOrder { get; set; } 
}

/// <summary>
///     Generic mapper that copies properties and generates UI items based on field attributes
/// </summary>
public static class AttributeMapper {
    /// <summary>
    ///     Copies all copyable properties from source to target
    /// </summary>
    public static void CopyTo<T>(T source, T target) where T : class {
        CopyTo(source, target, null);
    }

    /// <summary>
    ///     Copies properties from source to target, optionally filtered by group
    /// </summary>
    public static void CopyTo<T>(T source, T target, string? group) where T : class {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        var type = typeof(T);
        var propertiesToCopy = GetCopyableProperties(type, group);

        foreach (var property in propertiesToCopy) {
            var value = property.GetValue(source);
            property.SetValue(target, value);
        }
    }

    /// <summary>
    ///     Generates grouped PanelColorItem collection for all copyable Color properties
    /// </summary>
    public static ObservableCollection<ColorItemGroup> GenerateGroupedColorItems(Panel instance) {
        return GenerateGroupedColorItems(instance, null);
    }

    /// <summary>
    ///     Generates grouped PanelColorItem collection for copyable Color properties, optionally filtered by group
    /// </summary>
    public static ObservableCollection<ColorItemGroup> GenerateGroupedColorItems(Panel instance, string? group) {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        var type = typeof(Panel);
        var colorProperties = GetCopyableColorProperties(type, group);

        var colorItemsWithSortOrder = new List<(PanelColorItem Item, int CategorySortOrder, int ItemSortOrder)>();

        foreach (var (property, attribute) in colorProperties) {
            var displayName = !string.IsNullOrEmpty(attribute.DisplayName)
                ? attribute.DisplayName
                : SplitCamelCase(property.Name);

            var category = !string.IsNullOrEmpty(attribute.Category)
                ? attribute.Category
                : attribute.Group;

            var getter = CreatePanelGetter(property);
            var setter = CreatePanelSetter(property);

            var colorItem = new PanelColorItem(instance, category, displayName, property.Name, getter, setter);
            colorItemsWithSortOrder.Add((colorItem, attribute.CategorySortOrder, attribute.ItemSortOrder));
        }

        // Group by category and sort
        var groupedItems = colorItemsWithSortOrder
                          .GroupBy(x => new { x.Item.Category, x.CategorySortOrder })
                          .OrderBy(g => g.Key.CategorySortOrder)
                          .ThenBy(g => g.Key.Category)
                          .Select(g => new ColorItemGroup(
                                      g.Key.Category,
                                      g.Key.CategorySortOrder,
                                      g.OrderBy(x => x.ItemSortOrder).ThenBy(x => x.Item.LabelText).Select(x => x.Item)))
                          .ToList();

        return new ObservableCollection<ColorItemGroup>(groupedItems);
    }

    /// <summary>
    ///     Generates flat PanelColorItem collection for backward compatibility
    /// </summary>
    public static ObservableCollection<PanelColorItem> GenerateColorItems(Panel instance) {
        return GenerateColorItems(instance, null);
    }

    /// <summary>
    ///     Generates flat PanelColorItem collection for copyable Color properties, optionally filtered by group
    /// </summary>
    public static ObservableCollection<PanelColorItem> GenerateColorItems(Panel instance, string? group) {
        var groupedItems = GenerateGroupedColorItems(instance, group);
        var flatItems = groupedItems.SelectMany(g => g).ToList();
        return new ObservableCollection<PanelColorItem>(flatItems);
    }

    /// <summary>
    ///     Gets all available groups for copyable properties on the specified type
    /// </summary>
    public static IEnumerable<string> GetGroups<T>() where T : class {
        var type = typeof(T);
        return type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                   .Select(f => f.GetCustomAttribute<CopyableAttribute>())
                   .Where(attr => attr != null)
                   .Select(attr => attr!.Group)
                   .Distinct();
    }

    private static IEnumerable<PropertyInfo> GetCopyableProperties(Type type, string? group) {
        var markedFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                               .Where(f => f.GetCustomAttribute<CopyableAttribute>() != null);

        if (!string.IsNullOrEmpty(group)) {
            markedFields = markedFields.Where(f => {
                var attr = f.GetCustomAttribute<CopyableAttribute>();
                return attr?.Group == group;
            });
        }

        var propertyNames = markedFields.Select(f => GetPropertyNameFromField(f.Name)).ToHashSet();

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(p => p.CanRead && p.CanWrite && propertyNames.Contains(p.Name));
    }

    private static IEnumerable<(PropertyInfo Property, CopyableAttribute Attribute)> GetCopyableColorProperties(Type type, string? group) {
        var markedFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                               .Where(f => f.GetCustomAttribute<CopyableAttribute>() != null);

        if (!string.IsNullOrEmpty(group)) {
            markedFields = markedFields.Where(f => {
                var attr = f.GetCustomAttribute<CopyableAttribute>();
                return attr?.Group == group;
            });
        }

        foreach (var field in markedFields) {
            var attr = field.GetCustomAttribute<CopyableAttribute>();
            var propertyName = GetPropertyNameFromField(field.Name);
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            // Only include Color properties
            if (property != null && property.CanRead && property.CanWrite &&
                property.PropertyType == typeof(Color) && attr != null) {
                yield return (property, attr);
            }
        }
    }

    private static Func<Panel, Color> CreatePanelGetter(PropertyInfo property) {
        return panel => (Color)property.GetValue(panel)!;
    }

    private static Action<Panel, Color> CreatePanelSetter(PropertyInfo property) {
        return (panel, color) => property.SetValue(panel, color);
    }

    private static string GetPropertyNameFromField(string fieldName) {
        if (fieldName.StartsWith("_") && fieldName.Length > 1) {
            return char.ToUpper(fieldName[1]) + fieldName.Substring(2);
        }
        return fieldName;
    }

    private static string SplitCamelCase(string input) {
        return Regex.Replace(input, "([A-Z])", " $1").Trim();
    }
}