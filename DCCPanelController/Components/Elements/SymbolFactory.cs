using System.Collections.ObjectModel;
using DCCPanelController.Components.Elements.ViewModels;
using DCCPanelController.Components.Elements.Views;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Elements;

/// <summary>
/// The 'SymbolFactory' returns a list of available symbols and a View for each of the symbols.
/// A Symbol is an element that can be displayed in the toolbar and can be selected. The use of a Symbol
/// then allows us to create an Element View with an associated Element ViewModel and to place this
/// element on the Panel.  
/// </summary>
public static class SymbolFactory {

    private static readonly Dictionary<string, ImageSource> Symbols = new() {
        { "Straight",       ImageSource.FromFile("straight.png") },
        { "Terminate",      ImageSource.FromFile("terminate.png") },
        { "Crossing",       ImageSource.FromFile("crossing.png") },
        { "Left",           ImageSource.FromFile("angleleft.png") },
        { "Right",          ImageSource.FromFile("angleright.png") },
        { "Turnout(L)",     ImageSource.FromFile("turnoutleft.png") },
        { "Turnout(R)",     ImageSource.FromFile("turnoutright.png") },
        { "Wye-Junction",   ImageSource.FromFile("yjunction.png") },
        { "Text",           ImageSource.FromFile("yjunction.png") },
    };
    
    public static SymbolViewModel? CreateView(string name) {
        return Symbols.TryGetValue(name, out var symbol) ? new SymbolViewModel(name, symbol) : null;
    }

    public static List<SymbolViewModel> AvailableSymbols() {
        var symbols = Symbols.Select(symbol => CreateView(symbol.Key)).Where(view => view is not null).ToList();
        return symbols!;
    }

    public static async IAsyncEnumerable<SymbolViewModel> AvailableSymbolsAsync() {
        foreach (var symbolViewModel in Symbols.Select(symbol => CreateView(symbol.Key)).OfType<SymbolViewModel>()) {
            yield return symbolViewModel;
        }
    }

}