namespace DCCPanelController.Helpers;

public static class PropertyHelper {
    public static int GetEnumValueIndex(object obj, string fieldName) {
        // Get the type of the object
        var objType = obj.GetType();

        // Fetch the property info based on the property name
        var property = objType.GetProperty(fieldName);

        if (property == null)
            throw new ArgumentException($"Property '{fieldName}' not found on object of type {objType.Name}.");

        // Check if the property is an enum
        if (!property.PropertyType.IsEnum)
            throw new InvalidOperationException($"Property '{fieldName}' is not an enum.");

        // Get the current value of the enum
        var enumValue = property.GetValue(obj);

        if (enumValue == null) return 0;

        // Get all enum values for the property's type
        var enumValues = Enum.GetValues(property.PropertyType).Cast<object>().ToList();

        // Find the index of the current value within the enum values
        return enumValues.IndexOf(enumValue);
    }

    public static T? GetEnumPropertyValue<T>(object obj, string fieldName) where T : struct, Enum {
        // Get the type of the object
        var objType = obj.GetType();

        // Fetch the property info based on the property name
        var property = objType.GetProperty(fieldName);

        if (property == null)
            throw new ArgumentException($"Property '{fieldName}' not found on object of type {objType.Name}.");

        // Check if the property is an enum
        if (!property.PropertyType.IsEnum)
            throw new InvalidOperationException($"Property '{fieldName}' is not an enum.");

        // Get the current value of the enum
        return property.GetValue(obj) as T?;
    }

    public static void SetEnumPropertyValue(object obj, string propertyName, object enumValue) {
        // Get the type of the object
        var objType = obj.GetType();

        // Fetch the property by name
        var property = objType.GetProperty(propertyName);

        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' not found on object of type {objType.Name}.");

        // Check if the property is an Enum
        if (!property.PropertyType.IsEnum)
            throw new InvalidOperationException($"Property '{propertyName}' is not an enum.");

        // Validate the enum value type and convert if necessary
        if (!Enum.IsDefined(property.PropertyType, enumValue))
            throw new ArgumentException($"Value '{enumValue}' is not valid for enum type {property.PropertyType.Name}.");

        // Set the value
        //property.SetValue(obj, Enum.Parse(property.PropertyType, enumValue.ToString() ?? string.Empty));
        property.SetValue(obj, enumValue);
    }

    public static void SetPropertyValue<T>(object obj, string propertyName, object value) {
        // Get the type of the object
        var objType = obj.GetType();

        // Fetch the property by name
        var property = objType.GetProperty(propertyName);

        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' not found on object of type {objType.Name}.");

        // Set the value
        property.SetValue(obj, value);
    }

    public static T? GetPropertyValue<T>(object obj, string fieldName) {
        // Get the type of the object
        var objType = obj.GetType();

        // Fetch the property info based on the property name
        var property = objType.GetProperty(fieldName);

        if (property == null)
            throw new ArgumentException($"Property '{fieldName}' not found on object of type {objType.Name}.");

        // Get the current value of the enum
        return (property.GetValue(obj) is T? ? (T?)property.GetValue(obj) : default) ?? default(T);
    }
}