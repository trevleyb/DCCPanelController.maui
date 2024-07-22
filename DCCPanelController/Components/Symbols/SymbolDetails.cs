namespace DCCPanelController.Components.Symbols;

public class Symbols {
    public List<SymbolSet> Sets { get; init; } = [];
    public List<string> SetNames => Sets.Select(s => s.Name).ToList();
    public List<SymbolDetails> DetailsForSet(string set) => Sets.FirstOrDefault(s => s.Name == set)?.Symbols ?? [];
    public List<SymbolDetails> AllDetails() => Sets.SelectMany(set => set.Symbols).ToList();
    public SymbolDetails? GetByKey(string key) {
        return Sets.Select(set => set.Symbols.Find(d => d.Key == key)).OfType<SymbolDetails>().FirstOrDefault();
    }

    // We only store the set name at the top of the collection, but each 
    // symbol needs to know which set it is a part of. 
    public void ApplySets() {
        foreach (var set in Sets) {
            if (set.Name != "All") {
                foreach (var symbol in set.Symbols) {
                    symbol.Set = set.Name;
                }
            }
        }
        Sets.Insert(0, new SymbolSet() {Name = "All", Symbols = AllDetails() });
    }
}

public class SymbolSet {
    public string Name { get; set; } = string.Empty;
    public List<SymbolDetails> Symbols { get; init; } = [];
}

public class SymbolDetails {
    public SymbolDetails() {}
    public SymbolDetails(string set, string name, int height, int width, string image, string viewModel) {
        Set = set;
        Name = name;
        Height = height;
        Width = width;
        Image = image;
        ViewModel = viewModel;
    }
    public string Key => $"{Set}:{Name}";
    public string Set { get; set; }
    public string Name { get; set; }
    public int Height { get; set; } = 1;
    public int Width { get; set; } = 1;
    public string Image { get; set; }
    public string ViewModel { get; init; }
}



