using System.Diagnostics;
using System.Text.Json;

namespace DCCPanelController.Clients.Helpers;

public static class JsonHelper {
    // Utility methods for JSON parsing
    public static string GetStringProperty(this JsonElement element, string propertyName) {
        try {
            return (element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null) ?? string.Empty;
        } catch {
            Debug.WriteLine($"Failed to get 'string' property: {propertyName} from {element}");
            return string.Empty;
        }
    }

    public static int GetIntProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : 0;
        } catch {
            Debug.WriteLine($"Failed to get 'int' property: {propertyName} from {element}");
            return 0;
        }
    }

    public static double GetDoubleProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetDouble() : 0.0;
        } catch {
            Debug.WriteLine($"Failed to get 'double' property: {propertyName} from {element}");
            return 0;
        }
    }

    public static bool GetBoolProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) && prop.GetBoolean();
        } catch {
            Debug.WriteLine($"Failed to get 'bool' property: {propertyName} from {element}");
            return false;
        }
    }
    
    public static DateTime? GetTimeProperty(this JsonElement element, string propertyName) {
        try {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetDateTime() : null;
        } catch {
            Debug.WriteLine($"Failed to get 'bool' property: {propertyName} from {element}");
            return null;
        }
    }

}