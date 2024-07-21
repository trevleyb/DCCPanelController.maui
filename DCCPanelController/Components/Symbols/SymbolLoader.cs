using System.Text.Json;

namespace DCCPanelController.Components.Symbols;

public static class SymbolLoader {

    private static Symbols? _loaded = null;
    public static Symbols Symbols => _loaded ??= LoadFromJson("symbols.json");

    public static Symbols LoadFromJson(string filePath) {
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException($"The file {filePath} does not exist.");
        }

        var jsonContent = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<Symbols>(jsonContent, options)
                   ?? throw new InvalidOperationException("Failed to deserialize JSON to SymbolRoot.");

        data.ApplySets();
        return data;
    }
}