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

        FixDataAndUpdateSetDetails(data);
        return data;
    }
    
    // We only store the set name at the top of the collection, but each 
    // symbol needs to know which set it is a part of. 
    private static void FixDataAndUpdateSetDetails(Symbols data) {
        foreach (var set in data.Sets) {
            if (set.Name != "All") {
                foreach (var symbol in set.Symbols) {
                    symbol.Set = set.Name;
                }
            }
        }
        data.Sets.Insert(0, new SymbolSet() {Name = "All", Symbols = data.Sets.SelectMany(set => set.Symbols).ToList() });
    }
}