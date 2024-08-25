using System.Reflection;
using System.Collections;

namespace DCCPanelController.Helpers;

public static class ObjectCloner {
    public static T? Clone<T>(T source) {
        ArgumentNullException.ThrowIfNull(source);
        return (T)CloneObject(source) ?? default(T);
    }

    private static object? CloneObject(object? source) {
        if (source == null) return null;
        var type = source.GetType();

        // Handle simple types and strings
        if (type.IsPrimitive || type.IsEnum || type == typeof(string)) {
            return source;
        }

        // Handle arrays
        if (type.IsArray) {
            var elementType = type.GetElementType();
            var sourceArray = (Array)source;
            if (elementType != null) {
                var copiedArray = Array.CreateInstance(elementType, sourceArray.Length);
                for (var i = 0; i < sourceArray.Length; i++) {
                    copiedArray.SetValue(CloneObject(sourceArray.GetValue(i)), i);
                }
                return copiedArray;
            }
        }

        // Handle collections
        if (source is IList sourceList) {
            var copiedList = (IList)Activator.CreateInstance(source.GetType())!;

            foreach (var item in sourceList) {
                copiedList.Add(CloneObject(item));
            }
            return copiedList;
        }

        // Handle other reference types
        var copy = Activator.CreateInstance(type);
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if (property.CanWrite) {
                var propertyValue = property.GetValue(source);
                property.SetValue(copy, CloneObject(propertyValue));
            }
        }
        return copy;
    }
}