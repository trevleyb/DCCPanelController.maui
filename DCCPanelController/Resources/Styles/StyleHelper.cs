namespace DCCPanelController.Resources.Styles;

public static class StyleHelper {
    public static Color FromStyle(string styleName, Color? defaultColor = null) {
        try {
            if (Application.Current?.Resources[styleName] is Color styleColor) return styleColor;
        } catch { /* Don't worry about it */
        }
        return defaultColor ?? Colors.Black;
    }
}