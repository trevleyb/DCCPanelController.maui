using System.Text;
using System.Text.Json;
using DCCPanelController.Helpers.Logging;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Repository;

/// <summary>
/// This class is used to validate the JSON files and ensure they have the right tag in them to determine the type.
/// General only on Profile and Panel as these are the two things we can download and re-upload. 
/// </summary>
public static class JsonTagValidator {

    public const string TagPropertyName = "$type";
    
    public static bool HasTypeTag<T>(string json, string discriminatorProperty = TagPropertyName, StringComparison comparison = StringComparison.Ordinal) => HasTypeTag(json, typeof(T).Name, discriminatorProperty, comparison);

    public static bool HasTypeTag(string json, string expectedMarker, string discriminatorProperty = TagPropertyName, StringComparison comparison = StringComparison.Ordinal) {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrEmpty(expectedMarker)) return false;
        try {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return false;
            if (!root.TryGetProperty(discriminatorProperty, out var typeProp)) return false;

            var actual = typeProp.ValueKind == JsonValueKind.String ? typeProp.GetString() : null;
            return actual is { } && string.Equals(actual, expectedMarker, comparison);
        } catch (JsonException ex) {
            LogHelper.Logger.LogError(ex, "JsonTagValidator: Invalid JSON HasTypeTag: JsonException: {Message}", ex.Message);
            return false;
        }
    }

    public static bool HasTypeTagInFile<T>(string filePath, string discriminatorProperty = TagPropertyName, StringComparison comparison = StringComparison.Ordinal,
        Encoding? encoding = null) {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

        try {
            var json = File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
            return HasTypeTag<T>(json, discriminatorProperty, comparison);
        } catch {
            return false;
        }
    }

    public static async Task<bool> HasTypeTagInFileAsync<T>(string filePath, string discriminatorProperty = TagPropertyName, StringComparison comparison = StringComparison.Ordinal,
        Encoding? encoding = null,
        CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

        try {
            using var fs = File.OpenRead(filePath);
            using var reader = new StreamReader(fs, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            #if NET8_0_OR_GREATER
            var json = await reader.ReadToEndAsync(ct);
            #else
            var json = await reader.ReadToEndAsync();
            if (ct.IsCancellationRequested) return false;
            #endif
            return HasTypeTag<T>(json, discriminatorProperty, comparison);
        } catch {
            return false;
        }
    }
}
