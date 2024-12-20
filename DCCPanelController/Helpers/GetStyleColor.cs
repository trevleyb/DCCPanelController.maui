namespace DCCPanelController.Helpers;

public static class StyleColor {
    public static Color Get(string color, Color? defaultColor = null) {
        if (Application.Current is { } application) {
            if (application.Resources.TryGetValue(color, out var styleColor)) {
                return (Color)styleColor;
            }
        }
        return defaultColor ?? Colors.Black;
    }
}