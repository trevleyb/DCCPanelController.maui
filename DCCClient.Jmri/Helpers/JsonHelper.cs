using System.Text.Json;

namespace DccClients.Jmri.Helpers;

public static class JsonHelper {
    // Utility methods for JSON parsing
    public static string GetStringProperty(this JsonElement element, string propertyName) {
        try {
            return (element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null) ?? string.Empty;
        } catch {
            Console.WriteLine($"Failed to get 'string' property: {propertyName} from {element}");
            return string.Empty;
        }
    }

    public static int GetIntProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : 0;
        } catch {
            Console.WriteLine($"Failed to get 'int' property: {propertyName} from {element}");
            return 0;
        }
    }

    public static double GetDoubleProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetDouble() : 0.0;
        } catch {
            Console.WriteLine($"Failed to get 'double' property: {propertyName} from {element}");
            return 0;
        }
    }

    public static bool GetBoolProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) && prop.GetBoolean();
        } catch {
            Console.WriteLine($"Failed to get 'bool' property: {propertyName} from {element}");
            return false;
        }
    }
}