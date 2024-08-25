using System.Text.Json;

namespace DCCPanelController.Tracks.StyleManager;

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
        if (string.IsNullOrEmpty(styleName)) styleName = "default";
        return _styles.TryGetValue(styleName, out var style) ? style : SvgDefaultStyles.GetStyle(styleName) ?? new SvgStyle(styleName);
    }
}