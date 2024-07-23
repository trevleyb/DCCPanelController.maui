namespace DCCPanelController.Components.Symbols;

/// <summary>
/// The 'SymbolFactory' returns a list of available symbols and a View for each of the symbols.
/// A Symbol is an element that can be displayed in the toolbar and can be selected. The use of a Symbol
/// then allows us to create an Element View with an associated Element ViewModel and to place this
/// element on the Panel.  
/// </summary>
public static class SymbolFactory {

    public static SymbolViewModel CreateView(string key) {
        var sd = SymbolLoader.Symbols.GetByKey(key);
        if (sd == null) throw new KeyNotFoundException($"Could not find a Symbol with a key of: {key}");
        return new SymbolViewModel(sd);
    }

    public static List<SymbolViewModel> AvailableSymbols(string set) {
        var symbols = SymbolLoader.Symbols.DetailsForSet(set).Select(symbol => CreateView(symbol.Key)).ToList();
        return symbols!;
    }

    public static List<string> AvailableSets => SymbolLoader.Symbols.SetNames;
}
