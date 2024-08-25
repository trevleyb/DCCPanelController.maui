using System.Reflection;

namespace DCCPanelController.Components.TrackPieces.ImageManager;

public static class SvgImageFinder {
    
    private static readonly object SyncRoot = new();
    private static List<string>? _availableSymbols;
    public static List<string> AvailableSymbols {
        get {
            lock (SyncRoot) {
                if (_availableSymbols is null) {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceNames = assembly.GetManifestResourceNames();
                    _availableSymbols = resourceNames.Where(name => name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            return _availableSymbols ?? throw new ApplicationException("No SVG Symbols for Tracks found in this assembly.");
        }
    }

    public static string GetFullPathOfResource(string filename) {
        return AvailableSymbols.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x.Contains(filename)) ?? throw new FileNotFoundException($"File not found: {filename}");
    }
}