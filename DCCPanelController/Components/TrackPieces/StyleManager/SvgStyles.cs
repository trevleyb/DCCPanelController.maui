using System.Text.Json;

namespace DCCPanelController.Components.TrackPieces.SVGManager;

public static class SvgStyles {
    
    private static Dictionary<string, SvgStyle> _styles = [];

    // Export of Import our collection of Styles
    // ----------------------------------------------------------------------------------------
    public static string Export() => JsonSerializer.Serialize(_styles, new JsonSerializerOptions { WriteIndented = false });
    public static void Import(string jsonString) {
        if (!string.IsNullOrWhiteSpace(jsonString)) {
            var styles = JsonSerializer.Deserialize<Dictionary<string, SvgStyle>>(jsonString);
            _styles = styles ?? new Dictionary<string, SvgStyle>();
        }
    }

    public static List<string> AvailableStyles => _styles.Keys?.ToList() ?? new List<string>(); 
    public static SvgStyle GetStyle(string styleName) {
        return _styles.ContainsKey(styleName) ? _styles[styleName] : SvgDefaultStyles.GetStyle(styleName) ?? new SvgStyle(styleName);
    }
}