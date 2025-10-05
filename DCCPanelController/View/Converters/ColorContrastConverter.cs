using System.Globalization;

namespace DCCPanelController.View.Converters;

// Single-value: value = main color, ConverterParameter = background (Color, "#RRGGBB", Brush, or a VisualElement via x:Reference)
public class ContrastToBackgroundConverter : IValueConverter
{
    public double MinContrast { get; set; } = 4.5;     // 3.0 is fine for icons
    public bool OnlyAdjustWhenSimilarTone { get; set; } = true;
    public double DarkThreshold { get; set; } = 0.33;  // luminance 0..1
    public double SimilarToneDelta { get; set; } = 0.12;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var main = ColorHelpers.ParseColor(value, Colors.Transparent);
        var bg   = ColorHelpers.ResolveBackground(parameter) ?? Colors.White;

        // already good enough? keep it
        if (ColorHelpers.Contrast(main, bg) >= MinContrast) return main;

        // only adjust when they are both dark or tones are very similar
        var lumMain = ColorHelpers.Luminance(main);
        var lumBg   = ColorHelpers.Luminance(bg);
        bool bothDark    = lumMain <= DarkThreshold && lumBg <= DarkThreshold;
        bool similarTone = Math.Abs(lumMain - lumBg) <= SimilarToneDelta;

        if (OnlyAdjustWhenSimilarTone && !(bothDark || similarTone))
            return main; // e.g., green on white stays green

        // Try black/white first
        var bestBW = ColorHelpers.BestOf(Colors.Black, Colors.White, bg, out var bestBWRatio);
        if (bestBWRatio >= MinContrast)
            return bestBW.WithAlpha(main.Alpha);

        // Fallback: invert original (keeps hue “feel”)
        var inv = new Color(1f - main.Red, 1f - main.Green, 1f - main.Blue, main.Alpha);
        return ColorHelpers.Contrast(inv, bg) >= bestBWRatio ? inv : bestBW.WithAlpha(main.Alpha);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}


// Multi-value: values[0] = main color, values[1] = background color (both bindable)
public class ContrastToBackgroundMultiConverter : IMultiValueConverter
{
    public double MinContrast { get; set; } = 4.5;
    public bool OnlyAdjustWhenSimilarTone { get; set; } = true;
    public double DarkThreshold { get; set; } = 0.33;
    public double SimilarToneDelta { get; set; } = 0.12;

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var main = ColorHelpers.ParseColor(values.ElementAtOrDefault(0), Colors.Transparent);
        var bg   = ColorHelpers.ParseColor(values.ElementAtOrDefault(1), Colors.White);

        if (ColorHelpers.Contrast(main, bg) >= MinContrast)
            return main;

        var lumMain = ColorHelpers.Luminance(main);
        var lumBg   = ColorHelpers.Luminance(bg);
        bool bothDark    = lumMain <= DarkThreshold && lumBg <= DarkThreshold;
        bool similarTone = Math.Abs(lumMain - lumBg) <= SimilarToneDelta;

        if (OnlyAdjustWhenSimilarTone && !(bothDark || similarTone))
            return main;

        var bestBW = ColorHelpers.BestOf(Colors.Black, Colors.White, bg, out var bestBWRatio);
        if (bestBWRatio >= MinContrast)
            return bestBW.WithAlpha(main.Alpha);

        var inv = new Color(1f - main.Red, 1f - main.Green, 1f - main.Blue, main.Alpha);
        return ColorHelpers.Contrast(inv, bg) >= bestBWRatio ? inv : bestBW.WithAlpha(main.Alpha);
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => [value, null];
}

static class ColorHelpers {
    public static Color? ResolveBackground(object? parameter) {
        if (parameter is null) return null;
        if (parameter is VisualElement ve) return ve.BackgroundColor;
        return ParseColor(parameter, Colors.White);
    }

    public static Color ParseColor(object? input, Color fallback) {
        if (input is null) return fallback;
        if (input is Color c) return c;
        if (input is SolidColorBrush sb) return sb.Color;
        if (input is Brush and SolidColorBrush sb2) return sb2.Color;
        if (input is string s) {
            s = s.Trim();
            if (TryParseHex(s, out var hexC)) return hexC;

            // Named colors (e.g., "Red")
            var named = typeof(Colors).GetProperties()
                                      .FirstOrDefault(p => string.Equals(p.Name, s, StringComparison.OrdinalIgnoreCase));
            if (named?.GetValue(null) is Color nc) return nc;
        }
        return fallback;
    }

    static bool TryParseHex(string s, out Color color) {
        if (s.StartsWith("#")) s = s[1..];
        byte a = 255, r = 0, g = 0, b = 0;
        try {
            if (s.Length == 3) // RGB (12-bit)
            {
                r = (byte)Convert.ToInt32(new string(s[0], 2), 16);
                g = (byte)Convert.ToInt32(new string(s[1], 2), 16);
                b = (byte)Convert.ToInt32(new string(s[2], 2), 16);
            } else if (s.Length == 6) // RRGGBB
            {
                r = Convert.ToByte(s.Substring(0, 2), 16);
                g = Convert.ToByte(s.Substring(2, 2), 16);
                b = Convert.ToByte(s.Substring(4, 2), 16);
            } else if (s.Length == 8) // AARRGGBB
            {
                a = Convert.ToByte(s.Substring(0, 2), 16);
                r = Convert.ToByte(s.Substring(2, 2), 16);
                g = Convert.ToByte(s.Substring(4, 2), 16);
                b = Convert.ToByte(s.Substring(6, 2), 16);
            } else {
                color = Colors.Transparent;
                return false;
            }
            color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            return true;
        } catch {
            color = Colors.Transparent;
            return false;
        }
    }

    // WCAG relative luminance
    public static double Luminance(Color c) {
        static double Chan(double x) => x <= 0.03928 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4);
        var r = Chan(c.Red);
        var g = Chan(c.Green);
        var b = Chan(c.Blue);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    public static double Contrast(Color a, Color b) {
        var l1 = Luminance(a) + 0.05;
        var l2 = Luminance(b) + 0.05;
        return l1 > l2 ? l1 / l2 : l2 / l1;
    }

    public static Color BestOf(Color c1, Color c2, Color bg, out double bestRatio) {
        var r1 = Contrast(c1, bg);
        var r2 = Contrast(c2, bg);
        if (r1 >= r2) {
            bestRatio = r1;
            return c1;
        }
        bestRatio = r2;
        return c2;
    }
}