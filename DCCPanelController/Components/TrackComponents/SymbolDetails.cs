using System.Diagnostics;

namespace DCCPanelController.Components.TrackComponents;

public class Symbols {
    public List<SymbolSet> Sets { get; init; } = [];
    public List<string> SetNames => Sets.Select(s => s.Name).ToList();
    public List<SymbolDetails> DetailsForSet(string set) => Sets.FirstOrDefault(s => s.Name == set)?.Symbols ?? [];
    public SymbolDetails? GetByKey(string key) {
        return Sets.Select(set => set.Symbols.Find(d => d.Key == key)).OfType<SymbolDetails>().FirstOrDefault();
    }
}

[DebuggerDisplay("{Name} with {Symbols.Count} symbols")]
public class SymbolSet {
    public string Name { get; set; } = string.Empty;
    public List<SymbolDetails> Symbols { get; init; } = [];
}

[DebuggerDisplay("{Key}: {Name}")]
public class SymbolDetails {
    public SymbolDetails() {}
    public string Key => $"{Set}:{ID}";
    public string Set { get; set; }
    public string ID { get; set; }
    public string Name { get; set; }
    public string ViewModel { get; init; }
    public string Image { get; set; }
    public string Closed { get; set; }
    public string Thrown { get; set; }
    public int ZIndex { get; set; } = 0;
    public int Height { get; set; } = 1;
    public int Width { get; set; } = 1;
}



