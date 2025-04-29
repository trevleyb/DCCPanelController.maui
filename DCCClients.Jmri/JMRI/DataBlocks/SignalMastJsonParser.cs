using System.Text.Json;
namespace DCCClients.Jmri.JMRI.DataBlocks; // Root structure that contains the turnout data

public static class SignalMastParser {
    public static JmriSignalMastData? ParseSignalMastData(string jsonString) {
        // The string appears to have a prefix "ValueKind = Object : " that we need to remove
        string cleanedJson = jsonString;

        // Check if the string contains the prefix and remove it
        if (cleanedJson.Contains("ValueKind = Object : ")) {
            cleanedJson = cleanedJson.Replace("ValueKind = Object : ", "");
        }

        // The JSON appears to be wrapped in quotes, so we need to remove the enclosing quotes
        if (cleanedJson.StartsWith("\"") && cleanedJson.EndsWith("\"")) {
            cleanedJson = cleanedJson.Substring(1, cleanedJson.Length - 2);
        }

        // JSON strings within the text are escaped, so we need to unescape them
        cleanedJson = cleanedJson.Replace("\\\"", "\"");

        // Parse the JSON into our structure
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<JmriSignalMastData>(cleanedJson, options);
    }
}