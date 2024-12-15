using System.Collections;
using System.Reflection;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Model.Tracks.Base;

namespace DCCPanelController.Helpers;

public static class ObjectCloner {
    public static T? Clone<T>(T source) {
        ArgumentNullException.ThrowIfNull(source);
        return (T)CloneObject(source)! ?? default(T);
    }

    private static object? CloneObject(object? source) {
        if (source == null) return null;
        var type = source.GetType();

        // Handle simple types and strings
        if (type.IsPrimitive || type.IsEnum || type == typeof(Color) || type == typeof(string)) {
            return source;
        }

        // Handle arrays
        if (type.IsArray) {
            var elementType = type.GetElementType();
            var sourceArray = (Array)source;
            if (elementType != null) {
                try {
                    var copiedArray = Array.CreateInstance(elementType, sourceArray.Length);
                    for (var i = 0; i < sourceArray.Length; i++) {
                        copiedArray.SetValue(CloneObject(sourceArray.GetValue(i)), i);
                    }
                    return copiedArray;
                } catch (Exception e) {
                    Console.WriteLine($"Unable to clone '{type.ToString()}': {e.Message}");
                    throw;
                }
            }
        }

        // Handle collections
        if (source is IList sourceList) {
            try {
                var copiedList = (IList)Activator.CreateInstance(source.GetType())!;
                foreach (var item in sourceList) {
                    copiedList.Add(CloneObject(item));
                }

                return copiedList;
            } catch (Exception e) {
                Console.WriteLine($"Unable to clone '{type.ToString()}': {e.Message}");
                throw;
            }
        }

        // Handle other reference types
        try {
            var copy = Activator.CreateInstance(type);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (property is { CanWrite: true, CanRead: true } && !Attribute.IsDefined(property, typeof(DoNotCloneAttribute))) {
                    var propertyValue = property.GetValue(source);
                    property.SetValue(copy, CloneObject(propertyValue));
                }
            }
            return copy;
        } catch (Exception e) {
            Console.WriteLine($"Unable to clone '{type.ToString()}': {e.Message}");
            throw;
        }
    }

    //public static void UpdateOriginal<T>(T original, T modified) => UpdateOriginal(original, modified);

    public static void UpdateOriginal(object original, object modified) {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(modified);
        var originalType = original.GetType();
        var modifiedType = modified.GetType();
        if (originalType != modifiedType) {
            throw new ArgumentException("Original and modified objects must be of the same type.");
        }

        var type = original.GetType();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            Console.Write($"Updating:{property.Name} \t");
            if (property is { CanWrite: true, CanRead: true } && !Attribute.IsDefined(property, typeof(DoNotCloneAttribute))) {
                var propertyType = property.PropertyType;
                var originalValue = property.GetValue(original);
                var modifiedValue = property.GetValue(modified);
                Console.WriteLine($"Original: {originalValue} Modified: {modifiedValue}");

                if (propertyType.IsArray) {
                    if (originalValue != null && modifiedValue != null) {
                        var originalArray = (Array)originalValue;
                        var modifiedArray = (Array)modifiedValue;
                        if (originalArray.Length == modifiedArray.Length) {
                            for (var i = 0; i < originalArray.Length; i++) {
                                var originalElement = originalArray.GetValue(i);
                                var modifiedElement = modifiedArray.GetValue(i);

                                if (!Equals(originalElement, modifiedElement)) {
                                    if (originalElement != null && modifiedElement != null) {
                                        UpdateOriginal(originalElement, modifiedElement);
                                    } else {
                                        originalArray.SetValue(modifiedElement, i);
                                    }
                                }
                            }
                        }
                    } else {
                        property.SetValue(original, modifiedValue);
                    }
                } else if (typeof(IList).IsAssignableFrom(propertyType)) {
                    if (originalValue != null && modifiedValue != null) {
                        var originalList = (IList)originalValue;
                        var modifiedList = (IList)modifiedValue;

                        if (originalList.Count != modifiedList.Count) {
                            property.SetValue(original, modifiedList);
                        } else {
                            for (var i = 0; i < originalList.Count; i++) {
                                var originalItem = originalList[i];
                                var modifiedItem = modifiedList[i];

                                if (!Equals(originalItem, modifiedItem)) {
                                    if (originalItem != null && modifiedItem != null) {
                                        UpdateOriginal(originalItem, modifiedItem);
                                    } else {
                                        originalList[i] = modifiedItem;
                                    }
                                }
                            }
                        }
                    }
                } else if (propertyType.IsPrimitive || propertyType == typeof(string)) {
                    if (!Equals(originalValue, modifiedValue)) {
                        property.SetValue(original, modifiedValue);
                    }
                } else if (originalValue != null && modifiedValue != null) {
                    UpdateOriginal(originalValue, modifiedValue);
                } else {
                    property.SetValue(original, modifiedValue);
                }
            } else {
                Console.WriteLine("Not a property");
            }
        }
    }
}