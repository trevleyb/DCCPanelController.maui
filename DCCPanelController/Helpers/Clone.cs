using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.Helpers;

public static class ObjectCloner {
    // Cache for reflection data to improve performance
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]>                    PropertyCache               = new();
    private static readonly ConcurrentDictionary<(Type, HashSet<string>), PropertyInfo[]> PropertyCacheWithExclusions = new();
    private static readonly ConcurrentDictionary<Type, bool>                              SimpleTypeCache             = new();

    /// <summary>
    ///     Creates a deep clone of the specified object
    /// </summary>
    /// <typeparam name="T">The type of object to clone</typeparam>
    /// <param name="source">The source object to clone</param>
    /// <param name="excludeProperties">Property names to exclude from cloning</param>
    /// <returns>A deep clone of the source object</returns>
    public static T? Clone<T>(T source, params string[] excludeProperties) {
        ArgumentNullException.ThrowIfNull(source);
        var exclusions = new HashSet<string>(excludeProperties);
        return(T?)CloneObject(source, new Dictionary<object, object>(), exclusions);
    }

    /// <summary>
    ///     Creates a deep clone of the specified object
    /// </summary>
    /// <typeparam name="T">The type of object to clone</typeparam>
    /// <param name="source">The source object to clone</param>
    /// <returns>A deep clone of the source object</returns>
    public static T? Clone<T>(T source) {
        ArgumentNullException.ThrowIfNull(source);
        return(T?)CloneObject(source, new Dictionary<object, object>(), new HashSet<string>());
    }

    /// <summary>
    ///     Copies all cloneable properties from source to target
    /// </summary>
    /// <param name="source">Source object to copy from</param>
    /// <param name="target">Target object to copy to</param>
    /// <param name="excludeProperties">Property names to exclude from cloning</param>
    public static void CloneProperties(object source, object target, params string[] excludeProperties) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var sourceType = source.GetType();
        var targetType = target.GetType();

        if (sourceType != targetType) {
            throw new ArgumentException("Source and target objects must be of the same type.");
        }

        var exclusions = new HashSet<string>(excludeProperties);
        var properties = GetCloneableProperties(sourceType, exclusions);

        foreach (var property in properties) {
            try {
                var value = property.GetValue(source);
                var clonedValue = CloneObject(value, new Dictionary<object, object>(), exclusions);
                property.SetValue(target, clonedValue);
            } catch (Exception ex) {
                Console.WriteLine($"Failed to clone property {property.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Copies all cloneable properties from source to target (without exclusions)
    /// </summary>
    /// <param name="source">Source object to copy from</param>
    /// <param name="target">Target object to copy to</param>
    public static void CloneProperties(object source, object target) => CloneProperties(source, target, Array.Empty<string>());

    private static object? CloneObject(object? source, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        if (source == null) return null;

        if (source is ButtonActions or TurnoutActions) {
            Console.WriteLine("Cloning actions");
        }
        
        var type = source.GetType();

        // Handle circular references
        if (referenceMap.TryGetValue(source, out var existingClone)) {
            return existingClone;
        }

        // Handle simple types (primitives, enums, strings, Color, etc.)
        if (IsSimpleType(type)) {
            return source;
        }

        // Handle DateTime, DateTimeOffset, TimeSpan, Guid
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid)) {
            return source;
        }

        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
            return source;
        }

        // Handle arrays
        if (type.IsArray) {
            return CloneArray((Array)source, referenceMap, exclusions);
        }

        // Handle generic collections (List<T>, Dictionary<K,V>, etc.)
        if (IsGenericCollection(type)) {
            return CloneGenericCollection(source, type, referenceMap, exclusions);
        }

        // Handle non-generic collections (ArrayList, etc.)
        if (source is IList sourceList) {
            return CloneList(sourceList, referenceMap, exclusions);
        }

        // Handle dictionaries
        if (source is IDictionary sourceDictionary) {
            return CloneDictionary(sourceDictionary, referenceMap, exclusions);
        }

        // Handle reference types (classes)
        return CloneReferenceType(source, type, referenceMap, exclusions);
    }

    private static bool IsSimpleType(Type type) => SimpleTypeCache.GetOrAdd(type, t =>
        t.IsPrimitive ||
        t.IsEnum ||
        t == typeof(string) ||
        t == typeof(Color) ||
        t == typeof(decimal) ||
        t == typeof(DateTime) ||
        t == typeof(DateTimeOffset) ||
        t == typeof(TimeSpan) ||
        t == typeof(Guid)
    );

    private static bool IsGenericCollection(Type type) => type.IsGenericType && (
        typeof(IList<>).IsAssignableFromGenericDefinition(type) ||
        typeof(ICollection<>).IsAssignableFromGenericDefinition(type) ||
        typeof(IDictionary<,>).IsAssignableFromGenericDefinition(type)
    );

    private static bool IsAssignableFromGenericDefinition(this Type genericType, Type type) {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType) {
            return true;
        }

        return type.GetInterfaces()
                   .Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
    }

    private static object? CloneArray(Array sourceArray, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        var elementType = sourceArray.GetType().GetElementType()!;
        var copiedArray = Array.CreateInstance(elementType, sourceArray.Length);

        referenceMap[sourceArray] = copiedArray;

        for (var i = 0; i < sourceArray.Length; i++) {
            var element = sourceArray.GetValue(i);
            var clonedElement = CloneObject(element, referenceMap, exclusions);
            copiedArray.SetValue(clonedElement, i);
        }

        return copiedArray;
    }

    private static object? CloneGenericCollection(object source, Type type, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        try {
            var copy = Activator.CreateInstance(type);
            if (copy == null) return null;

            referenceMap[source] = copy;

            // Handle List<T>, Collection<T>, etc.
            if (source is IList sourceList && copy is IList copyList) {
                foreach (var item in sourceList) {
                    var clonedItem = CloneObject(item, referenceMap, exclusions);
                    copyList.Add(clonedItem);
                }
                return copy;
            }

            // Handle Dictionary<K,V>
            if (source is IDictionary sourceDictionary && copy is IDictionary copyDictionary) {
                foreach (DictionaryEntry entry in sourceDictionary) {
                    var clonedKey = CloneObject(entry.Key, referenceMap, exclusions);
                    var clonedValue = CloneObject(entry.Value, referenceMap, exclusions);
                    if (clonedKey != null) copyDictionary.Add(clonedKey, clonedValue);
                }
                return copy;
            }

            // Handle other ICollection<T> types
            var addMethod = type.GetMethod("Add");
            if (addMethod != null && source is IEnumerable sourceEnumerable) {
                foreach (var item in sourceEnumerable) {
                    var clonedItem = CloneObject(item, referenceMap, exclusions);
                    addMethod.Invoke(copy, [clonedItem]);
                }
                return copy;
            }

            return copy;
        } catch (Exception ex) {
            Console.WriteLine($"Unable to clone generic collection '{type}': {ex.Message}");
            throw;
        }
    }

    private static object? CloneList(IList sourceList, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        try {
            var copy = (IList)Activator.CreateInstance(sourceList.GetType())!;
            referenceMap[sourceList] = copy;

            foreach (var item in sourceList) {
                var clonedItem = CloneObject(item, referenceMap, exclusions);
                copy.Add(clonedItem);
            }

            return copy;
        } catch (Exception ex) {
            Console.WriteLine($"Unable to clone list '{sourceList.GetType()}': {ex.Message}");
            throw;
        }
    }

    private static object? CloneDictionary(IDictionary sourceDictionary, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        try {
            var copy = (IDictionary)Activator.CreateInstance(sourceDictionary.GetType())!;
            referenceMap[sourceDictionary] = copy;

            foreach (DictionaryEntry entry in sourceDictionary) {
                var clonedKey = CloneObject(entry.Key, referenceMap, exclusions);
                var clonedValue = CloneObject(entry.Value, referenceMap, exclusions);
                if (clonedKey != null) copy.Add(clonedKey, clonedValue);
            }

            return copy;
        } catch (Exception ex) {
            Console.WriteLine($"Unable to clone dictionary '{sourceDictionary.GetType()}': {ex.Message}");
            throw;
        }
    }

    private static object? CloneReferenceType(object source, Type type, Dictionary<object, object> referenceMap, HashSet<string> exclusions) {
        try {
            var copy = Activator.CreateInstance(type);
            if (copy == null) return null;

            referenceMap[source] = copy;

            var properties = GetCloneableProperties(type, exclusions);
            foreach (var property in properties) {
                var propertyValue = property.GetValue(source);
                var clonedValue = CloneObject(propertyValue, referenceMap, exclusions);
                property.SetValue(copy, clonedValue);
            }

            return copy;
        } catch (Exception ex) {
            Console.WriteLine($"Unable to clone reference type '{type}': {ex.Message}");
            throw;
        }
    }

    private static PropertyInfo[] GetCloneableProperties(Type type, HashSet<string>? exclusions = null) {
        if (exclusions == null || exclusions.Count == 0) {
            return PropertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanRead &&
                             p.CanWrite &&
                             !Attribute.IsDefined(p, typeof(DoNotCloneAttribute)))
                 .ToArray()
            );
        }

        // Create a key for caching that includes the exclusions
        var cacheKey = (type, exclusions);

        return PropertyCacheWithExclusions.GetOrAdd(cacheKey, key =>
            key.Item1.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .Where(p => p.CanRead &&
                           p.CanWrite &&
                           !Attribute.IsDefined(p, typeof(DoNotCloneAttribute)) &&
                           !key.Item2.Contains(p.Name))
               .ToArray()
        );
    }

    /// <summary>
    ///     Updates all properties of the original object with values from the modified object
    /// </summary>
    /// <param name="original">The object to update</param>
    /// <param name="modified">The object containing new values</param>
    /// <param name="excludeProperties">Property names to exclude from updating</param>
    public static void UpdateOriginal(object original, object modified, params string[] excludeProperties) {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(modified);

        var originalType = original.GetType();
        var modifiedType = modified.GetType();

        if (originalType != modifiedType) {
            throw new ArgumentException("Original and modified objects must be of the same type.");
        }

        var exclusions = new HashSet<string>(excludeProperties);
        UpdateObject(original, modified, new HashSet<object>(), exclusions);
    }

    /// <summary>
    ///     Updates all properties of the original object with values from the modified object (without exclusions)
    /// </summary>
    /// <param name="original">The object to update</param>
    /// <param name="modified">The object containing new values</param>
    public static void UpdateOriginal(object original, object modified) => UpdateOriginal(original, modified, Array.Empty<string>());

    private static void UpdateObject(object original, object modified, HashSet<object> visited, HashSet<string> exclusions) {
        if (visited.Contains(original)) return; // Prevent infinite recursion
        visited.Add(original);

        var type = original.GetType();
        var properties = GetCloneableProperties(type, exclusions);

        foreach (var property in properties) {
            try {
                var originalValue = property.GetValue(original);
                var modifiedValue = property.GetValue(modified);

                if (ReferenceEquals(originalValue, modifiedValue)) continue;

                var propertyType = property.PropertyType;

                if (IsSimpleType(propertyType) || originalValue == null || modifiedValue == null) {
                    // For simple types or null values, just copy directly
                    if (!Equals(originalValue, modifiedValue)) {
                        property.SetValue(original, modifiedValue);
                    }
                } else if (propertyType.IsArray) {
                    UpdateArray(originalValue, modifiedValue, property, original, exclusions);
                } else if (originalValue is IList originalList && modifiedValue is IList modifiedList) {
                    UpdateList(originalList, modifiedList, exclusions);
                } else if (originalValue is IDictionary originalDict && modifiedValue is IDictionary modifiedDict) {
                    UpdateDictionary(originalDict, modifiedDict, exclusions);
                } else {
                    // For complex objects, recursively update properties
                    UpdateObject(originalValue, modifiedValue, visited, exclusions);
                }
            } catch (Exception ex) {
                Console.WriteLine($"Failed to update property {property.Name}: {ex.Message}");
            }
        }
    }

    private static void UpdateArray(object originalValue, object modifiedValue, PropertyInfo property, object original, HashSet<string> exclusions) {
        var originalArray = (Array)originalValue;
        var modifiedArray = (Array)modifiedValue;

        if (originalArray.Length != modifiedArray.Length) {
            // If arrays have different lengths, replace the entire array
            property.SetValue(original, modifiedArray);
        } else {
            // Update elements in place
            for (var i = 0; i < originalArray.Length; i++) {
                var originalElement = originalArray.GetValue(i);
                var modifiedElement = modifiedArray.GetValue(i);

                if (!Equals(originalElement, modifiedElement)) {
                    if (originalElement != null && modifiedElement != null &&
                        !IsSimpleType(originalElement.GetType())) {
                        UpdateObject(originalElement, modifiedElement, new HashSet<object>(), exclusions);
                    } else {
                        originalArray.SetValue(modifiedElement, i);
                    }
                }
            }
        }
    }

    private static void UpdateList(IList originalList, IList modifiedList, HashSet<string> exclusions) {
        if (originalList.Count != modifiedList.Count) {
            originalList.Clear();
            foreach (var item in modifiedList) {
                originalList.Add(item);
            }
        } else {
            for (var i = 0; i < originalList.Count; i++) {
                var originalItem = originalList[i];
                var modifiedItem = modifiedList[i];

                if (!Equals(originalItem, modifiedItem)) {
                    if (originalItem != null && modifiedItem != null &&
                        !IsSimpleType(originalItem.GetType())) {
                        UpdateObject(originalItem, modifiedItem, new HashSet<object>(), exclusions);
                    } else {
                        originalList[i] = modifiedItem;
                    }
                }
            }
        }
    }

    private static void UpdateDictionary(IDictionary originalDict, IDictionary modifiedDict, HashSet<string> exclusions) {
        // Remove keys that are no longer present
        var keysToRemove = originalDict.Keys.Cast<object?>().Where(key => key is { } && !modifiedDict.Contains(key)).ToList();
        foreach (var key in keysToRemove.OfType<object>()) {
            originalDict.Remove(key);
        }

        // Add or update existing keys
        foreach (DictionaryEntry entry in modifiedDict) {
            if (originalDict.Contains(entry.Key)) {
                var originalValue = originalDict[entry.Key];
                var modifiedValue = entry.Value;

                if (!Equals(originalValue, modifiedValue)) {
                    if (originalValue != null && modifiedValue != null &&
                        !IsSimpleType(originalValue.GetType())) {
                        UpdateObject(originalValue, modifiedValue, new HashSet<object>(), exclusions);
                    } else {
                        originalDict[entry.Key] = modifiedValue;
                    }
                }
            } else {
                originalDict[entry.Key] = entry.Value;
            }
        }
    }
}