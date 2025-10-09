namespace DCCPanelController.View.Helpers;

public record FontFace(string Family, string Style, string Alias, string Filename, bool IsDefault = false);

public static class FontCatalog {
    public static readonly string[] StyleOrder = {
        "Light",
        "Light Italic",
        "Regular",
        "Italic",
        "Medium",
        "SemiBold",
        "Bold",
        "Bold Italic",
    };

    private static FontFace NewFontFace(string family, string style, string alias, string filename, bool isDefault = false) {
        return new FontFace(family, style, $"{family}{style}", filename, isDefault);
    }
    
    private static readonly List<FontFace> _registeredFonts = new() {
        // Inter
        NewFontFace("Inter", "Thin", "Inter-Thin", "Inter-Thin.ttf"),
        NewFontFace("Inter", "Light", "Inter-Light", "Inter-Light.ttf"),
        NewFontFace("Inter", "Light Italic", "Inter-LightItalic", "Inter-LightItalic.ttf"),
        NewFontFace("Inter", "Regular", "Inter-Regular","Inter-Regular.ttf" , true),
        NewFontFace("Inter", "Italic", "Inter-Italic", "Inter-Italic.ttf"),
        NewFontFace("Inter", "Medium", "Inter-Medium", "Inter-Medium.ttf"),
        NewFontFace("Inter", "SemiBold", "Inter-SemiBold", "Inter-SemiBold.ttf"),
        NewFontFace("Inter", "Bold", "Inter-Bold", "Inter-Bold.ttf"),
        
        // JetBrains Mono
        NewFontFace("JetBrains Mono", "Light", "JBM-Light", "JetBrainsMono-Light.ttf"),
        NewFontFace("JetBrains Mono", "Light Italic", "JBM-LightItalic", "JetBrainsMono-LightItalic.ttf"),
        NewFontFace("JetBrains Mono", "Regular", "JBM-Regular", "JetBrainsMono-Regular.ttf", true),
        NewFontFace("JetBrains Mono", "Italic", "JBM-Italic","JetBrainsMono-Italic.ttf"),
        NewFontFace("JetBrains Mono", "Medium", "JBM-Medium","JetBrainsMono-Medium.ttf"),
        NewFontFace("JetBrains Mono", "SemiBold", "JBM-SemiBold","JetBrainsMono-SemiBold.ttf"),
        NewFontFace("JetBrains Mono", "Bold", "JBM-Bold","JetBrainsMono-Bold.ttf"),

        // Open Sans
        NewFontFace("Open Sans", "Light", "OpenSans-Light", "OpenSans-Light.ttf"),
        NewFontFace("Open Sans", "Light Italic", "OpenSans-LightItalic", "OpenSans-LightItalic.ttf"),
        NewFontFace("Open Sans", "Regular", "OpenSans-Regular", "OpenSans-Regular.ttf"  ,true),
        NewFontFace("Open Sans", "Italic", "OpenSans-Italic", "OpenSans-Italic.ttf"),
        NewFontFace("Open Sans", "Medium", "OpenSans-Medium", "OpenSans-Medium.ttf"),
        NewFontFace("Open Sans", "Medium Italic", "OpenSans-MediumItalic", "OpenSans-MediumItalic.ttf"),
        NewFontFace("Open Sans", "SemiBold", "OpenSans-SemiBold", "OpenSans-SemiBold.ttf"),
        NewFontFace("Open Sans", "SemiBold Italic", "OpenSans-SemiBoldItalic", "OpenSans-SemiBoldItalic.ttf"),
        NewFontFace("Open Sans", "Bold", "OpenSans-Bold", "OpenSans-Bold.ttf"),
        NewFontFace("Open Sans", "Bold Italic", "OpenSans-BoldItalic", "OpenSans-BoldItalic.ttf"),

        // Roboto
        NewFontFace("Roboto", "Light", "Roboto-Light", "Roboto-Light.ttf"),
        NewFontFace("Roboto", "Light Italic", "Roboto-LightItalic", "Roboto-LightItalic.ttf"),
        NewFontFace("Roboto", "Regular", "Roboto-Regular", "Roboto-Regular.ttf", true),
        NewFontFace("Roboto", "Italic", "Roboto-Italic", "Roboto-Italic.ttf"),
        NewFontFace("Roboto", "Medium", "Roboto-Medium", "Roboto-Medium.ttf"),
        NewFontFace("Roboto", "Medium Italic", "Roboto-MediumItalic", "Roboto-MediumItalic.ttf"),
        NewFontFace("Roboto", "SemiBold", "Roboto-SemiBold", "Roboto-SemiBold.ttf"),
        NewFontFace("Roboto", "SemiBold Italic", "Roboto-SemiBoldItalic", "Roboto-SemiBoldItalic.ttf"),
        NewFontFace("Roboto", "Bold", "Roboto-Bold", "Roboto-Bold.ttf"),
        NewFontFace("Roboto", "Bold Italic", "Roboto-BoldItalic", "Roboto-BoldItalic.ttf"),

        // Orbitron (no italics)
        NewFontFace("Orbitron", "Regular", "Orbitron-Regular", "Orbitron-Regular.ttf", true),
        NewFontFace("Orbitron", "Medium", "Orbitron-Medium", "Orbitron-Medium.ttf"),
        NewFontFace("Orbitron", "SemiBold", "Orbitron-SemiBold", "Orbitron-SemiBold.ttf"),
        NewFontFace("Orbitron", "Bold", "Orbitron-Bold", "Orbitron-Bold.ttf"),

        // VT323 (single face)
        NewFontFace("VT323", "Regular", "VT323-Regular", "VT323-Regular.ttf", true),

        // Press Start 2P (single face)
        NewFontFace("Press Start 2P", "Regular", "PressStart2P-Regular", "PressStart2P-Regular.ttf", true),

        // Dancing Script (no light/italic files in your set)
        NewFontFace("Dancing Script", "Regular", "Dancing-Regular", "DancingScript-Regular.ttf", true),
        NewFontFace("Dancing Script", "Medium", "Dancing-Medium","DancingScript-Medium.ttf"),
        NewFontFace("Dancing Script", "SemiBold", "Dancing-SemiBold", "DancingScript-SemiBold.ttf"),
        NewFontFace("Dancing Script", "Bold", "Dancing-Bold", "DancingScript-Bold.ttf"),
    };

    // Public read-only view
    public static IReadOnlyList<FontFace> RegisteredFonts => _registeredFonts;

    // Families for your Family picker
    public static IReadOnlyList<string> Families => RegisteredFonts.Select(f => f.Family).Distinct().OrderBy(x => x).ToList();

    // Styles available for a given Family, in a nice order
    public static IReadOnlyList<string> StylesFor(string family) {
        var styles = RegisteredFonts.Where(f => f.Family.Equals(family, StringComparison.OrdinalIgnoreCase))
                         .Select(f => f.Style)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();

        styles.Sort(CompareStyle);
        return styles;
    }

    // Get alias for (Family, Style) — throws if missing
    public static string GetAlias(string? family, string? style) {
        var alias = GetFontFaceAlias(family, style)?.Alias;
        alias ??= GetFontFaceAlias(DefaultFontFamily, DefaultStyleFor(DefaultFontFamily))?.Alias;
        alias ??= DefaultFontAlias;
        return alias;
    }

    public static FontFace? GetFontFace(string? alias) {
        alias ??= DefaultFontAlias;
        return RegisteredFonts.FirstOrDefault(f => f.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase));
    }

    public static FontFace? GetFontFaceAlias(string? family, string? style) {
        return (string.IsNullOrEmpty(family) || string.IsNullOrEmpty(style)) ? null : 
            RegisteredFonts.FirstOrDefault(f =>
            f.Family.Equals(family, StringComparison.OrdinalIgnoreCase) &&
            f.Style.Equals(style, StringComparison.OrdinalIgnoreCase));
    }

    public static string DefaultFontAlias => "OpenSansRegular"; 
    public static string DefaultFontFamily => Families.Contains("Open Sans") ? "Open Sans" : Families[0];
    public static string DefaultStyleFor(string family) {
        var def = RegisteredFonts.FirstOrDefault(f => f.Family.Equals(family, StringComparison.OrdinalIgnoreCase) && f.IsDefault)?.Style ?? null;
        def ??= StylesFor(family).FirstOrDefault(s => s.Equals("Regular", StringComparison.OrdinalIgnoreCase))
             ?? StylesFor(family).FirstOrDefault()
             ?? "Regular";

        return def;
    }

    // Comparison helper that respects StyleOrder, then falls back alphabetically
    private static int CompareStyle(string a, string b) {
        int ia = Array.FindIndex(StyleOrder, s => s.Equals(a, StringComparison.OrdinalIgnoreCase));
        int ib = Array.FindIndex(StyleOrder, s => s.Equals(b, StringComparison.OrdinalIgnoreCase));
        if (ia >= 0 && ib >= 0) return ia.CompareTo(ib);
        if (ia >= 0) return-1;
        if (ib >= 0) return 1;
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}