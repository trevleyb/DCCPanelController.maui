using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Helpers;

public static class GenericComparer {
    
    private static readonly HashSet<Type> SimpleTypes = [
        typeof(string), typeof(int), typeof(double), typeof(float), typeof(decimal),
        typeof(bool), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
        typeof(Guid), typeof(byte), typeof(short), typeof(long), typeof(char),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong)
    ];

    public static bool AreEqual<T>(T? obj1, T? obj2, GenericComparerOptions? options = null) {
        options ??= new GenericComparerOptions();
        var visited = new HashSet<(object, object)>();
        return AreEqualInternal(obj1, obj2, options, visited, 0);
    }

    private static bool CompareResult(bool result, string? message = "", object? obj1 = null, object? obj2 = null ) {
        //Console.WriteLine($"Compare Result: {result}=>{message} {obj1?.GetType()}:{obj1?.ToString()} {obj2?.GetType()}:{obj2?.ToString()}");
        return result;
    }
    
    private static bool AreEqualInternal(object? obj1, object? obj2, 
                                         GenericComparerOptions options,
                                         HashSet<(object, object)> visited, int depth) {
        // Handle null cases
        if (obj1 == null && obj2 == null) return CompareResult(true, "Both Objects are Null");
        if (obj1 == null || obj2 == null) return CompareResult(false, "One of the objects is null");

        // Check depth limit
        if (depth >= options.MaxDepth) return CompareResult(true,"Max Depth exceeded");

        // Check for circular references
        var objPair = (obj1, obj2);
        if (!visited.Add(objPair)) return CompareResult(true,"Have already visited this pair");

        // Different types are not equal
        var type = obj1.GetType();
        if (type != obj2.GetType()) return CompareResult(false, "Types of objects are not the same");

        // Handle simple types, enums, and Nullable<>
        if (IsSimpleType(type)) return CompareResult(obj1.Equals(obj2),"IsSimpleType Comparison",obj1,obj2);
        if (type.IsEnum) return CompareResult(obj1.Equals(obj2),"IsEnum Types Comparison",obj1,obj2);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return CompareResult(obj1.Equals(obj2),"Generic Types Comparison",obj1,obj2);

        // Handle collections
        if (obj1 is IEnumerable enum1 && obj2 is IEnumerable enum2 && type != typeof(string)) {
            return CompareResult(CollectionsAreEqual(enum1, enum2, options, visited, depth),"Collections Comparison",enum1,enum2);
        }

        // Handle complex objects using reflection
        return CompareResult(ObjectsAreEqual(obj1, obj2, options, visited, depth),"Complex Types Comparison",obj1,obj2);
    }

    private static bool CollectionsAreEqual(IEnumerable enum1, IEnumerable enum2,
                                            GenericComparerOptions options, HashSet<(object, object)> visited, int depth) {
        var list1 = enum1.Cast<object>().ToList();
        var list2 = enum2.Cast<object>().ToList();

        if (list1.Count != list2.Count) return false;

        // For ordered collections, compare in order
        if (options.CompareCollectionsOrdered) {
            for (var i = 0; i < list1.Count; i++) {
                if (!AreEqualInternal(list1[i], list2[i], options, visited, depth + 1))
                    return false;
            }
            return true;
        }

        // For unordered collections, try to match each item
        var used = new bool[list2.Count];
        foreach (var item1 in list1) {
            bool found = false;
            for (int i = 0; i < list2.Count; i++) {
                if (used[i]) continue;
                if (AreEqualInternal(item1, list2[i], options, visited, depth + 1)) {
                    used[i] = true;
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }

    private static bool ObjectsAreEqual(object obj1, object obj2, GenericComparerOptions options,
                                        HashSet<(object, object)> visited, int depth) {
        var type = obj1.GetType();
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

        if (options.IncludePrivateProperties)
            bindingFlags |= BindingFlags.NonPublic;

        var properties = type.GetProperties(bindingFlags)
                             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                             .Where(p => !ShouldSkipProperty(p, options));

        foreach (var property in properties) {
            try {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!AreEqualInternal(value1, value2, options, visited, depth + 1))
                    return false;
            } catch {
                // If we can't read a property, consider objects different
                if (options.ThrowOnPropertyAccessError) throw;
                return false;
            }
        }

        return true;
    }

    private static bool ShouldSkipProperty(PropertyInfo property, GenericComparerOptions options) {
        // Skip properties in the ignore list
        if (options.IgnoreProperties.Any(ignore =>
                    property.Name.Equals(ignore, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Skip properties with JsonIgnore attribute
        if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) return true;

        // Skip properties with specific attributes
        foreach (var attributeType in options.IgnorePropertiesWithAttributes) {
            if (property.GetCustomAttribute(attributeType) != null) return true;
        }

        // Skip properties of certain types
        return options.IgnorePropertyTypes.Any(type => type.IsAssignableFrom(property.PropertyType));
    }

    private static bool IsSimpleType(Type type) =>
        SimpleTypes.Contains(type) ||
        SimpleTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
}

public class GenericComparerOptions {
    public int MaxDepth { get; init; } = 10;
    public bool IncludePrivateProperties { get; init; } = false;
    public bool CompareCollectionsOrdered { get; init; } = true;
    public bool ThrowOnPropertyAccessError { get; init; } = false;

    public HashSet<string> IgnoreProperties { get; init; } = new(StringComparer.OrdinalIgnoreCase) { "Parent", "Navigation", "Panels", "Guid" };
    public HashSet<Type> IgnorePropertyTypes { get; init; } = [typeof(INavigation)];
    public HashSet<Type> IgnorePropertiesWithAttributes { get; set; } = [typeof(JsonIgnoreAttribute)];
}