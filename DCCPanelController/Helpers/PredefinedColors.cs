using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

namespace DCCPanelController.Helpers;

public class ColorOption {
    private static List<int>? validLevels;

    public required string Name { get; set; }
    public required Color Color { get; set; }
    public required Color ContrastColor { get; set; }
    public string Hex => Color.ToHex();
    public bool IsPrimary => IsPrimaryColor(Color, 5);

    /// <summary>
    ///     Determines if a color is a valid 'primary' color based on the number of combinations.
    /// </summary>
    /// <param name="color">The color to check.</param>
    /// <param name="levels">The number of levels (2: 0,255; 3: 0,128,255; etc.).</param>
    /// <returns>True if the color is valid, otherwise False.</returns>
    private static bool IsPrimaryColor(Color color, int levels) {
        // Generate the threshold levels (e.g., for 3 levels: 0, 128, 255)
        var step = 255 / (levels - 1) + 1;

        if (validLevels == null || validLevels.Count != levels) {
            validLevels = new List<int> { 0, 255 };
            for (var i = 1; i < levels - 1; i++) validLevels.Add(i * step - 1);
        }

        // Validate if the Red, Green, and Blue components are in the valid levels
        var red = (int)(color.Red * 255);
        var green = (int)(color.Green * 255);
        var blue = (int)(color.Blue * 255);

        return validLevels.Contains(red) && validLevels.Contains(green) && validLevels.Contains(blue);
    }
}

public static class PredefinedColors {
    private static readonly ReadOnlyCollection<ColorOption> _allColors = new(BuildAllColors());
    private static readonly ReadOnlyCollection<ColorOption> _selectableColors = new(BuildSelectableColors());

    public static ColorOption None => new() { Name = "None", Color = Colors.White, ContrastColor = Colors.Black };
    public static ColorOption Default => new() { Name = "Black", Color = Colors.Black, ContrastColor = Colors.White };

    public static ColorOption FindColor(Color value) {
        return _allColors.FirstOrDefault(color => color.Hex == value.ToHex()) ?? Default;
    }

    public static ReadOnlyCollection<ColorOption> AllColors() {
        return _allColors;
    }

    public static ReadOnlyCollection<ColorOption> SelectableColors() {
        return _selectableColors;
    }

    public static ReadOnlyCollection<ColorOption> BuildSelectableColors() {
        List<ColorOption> colors = new();
        var steps = new List<int> { 0, 128, 255 };

        foreach (var red in steps) {
            foreach (var green in steps) {
                foreach (var blue in steps) {
                    var color = Color.FromRgb(red, green, blue);
                    var foundColor = _allColors.FirstOrDefault(c => c.Hex == color.ToHex());
                    colors.Add(foundColor ?? new ColorOption { Name = color.ToHex(), Color = color, ContrastColor = Colors.White });
                }
            }
        }

        colors.Add(new ColorOption { Name = "None", Color = Colors.White, ContrastColor = Colors.Black });
        return new ReadOnlyCollection<ColorOption>(colors);
    }

    private static ReadOnlyCollection<ColorOption> BuildAllColors() {
        var colors = new List<ColorOption> {
            new() { Name = "AliceBlue", Color = Colors.AliceBlue, ContrastColor = Colors.Black },
            new() { Name = "AntiqueWhite", Color = Colors.AntiqueWhite, ContrastColor = Colors.Black },
            new() { Name = "Aqua", Color = Colors.Aqua, ContrastColor = Colors.Black },
            new() { Name = "Aquamarine", Color = Colors.Aquamarine, ContrastColor = Colors.Black },
            new() { Name = "Azure", Color = Colors.Azure, ContrastColor = Colors.Black },
            new() { Name = "Beige", Color = Colors.Beige, ContrastColor = Colors.Black },
            new() { Name = "Bisque", Color = Colors.Bisque, ContrastColor = Colors.Black },
            new() { Name = "Black", Color = Colors.Black, ContrastColor = Colors.White },
            new() { Name = "BlanchedAlmond", Color = Colors.BlanchedAlmond, ContrastColor = Colors.Black },
            new() { Name = "Blue", Color = Colors.Blue, ContrastColor = Colors.White },
            new() { Name = "BlueViolet", Color = Colors.BlueViolet, ContrastColor = Colors.White },
            new() { Name = "Brown", Color = Colors.Brown, ContrastColor = Colors.White },
            new() { Name = "BurlyWood", Color = Colors.BurlyWood, ContrastColor = Colors.Black },
            new() { Name = "CadetBlue", Color = Colors.CadetBlue, ContrastColor = Colors.Black },
            new() { Name = "Chartreuse", Color = Colors.Chartreuse, ContrastColor = Colors.Black },
            new() { Name = "Chocolate", Color = Colors.Chocolate, ContrastColor = Colors.White },
            new() { Name = "Coral", Color = Colors.Coral, ContrastColor = Colors.Black },
            new() { Name = "CornflowerBlue", Color = Colors.CornflowerBlue, ContrastColor = Colors.Black },
            new() { Name = "Cornsilk", Color = Colors.Cornsilk, ContrastColor = Colors.Black },
            new() { Name = "Crimson", Color = Colors.Crimson, ContrastColor = Colors.White },
            new() { Name = "Cyan", Color = Colors.Cyan, ContrastColor = Colors.Black },
            new() { Name = "DarkBlue", Color = Colors.DarkBlue, ContrastColor = Colors.White },
            new() { Name = "DarkCyan", Color = Colors.DarkCyan, ContrastColor = Colors.White },
            new() { Name = "DarkGoldenrod", Color = Colors.DarkGoldenrod, ContrastColor = Colors.Black },
            new() { Name = "DarkGray", Color = Colors.DarkGray, ContrastColor = Colors.Black },
            new() { Name = "DarkGreen", Color = Colors.DarkGreen, ContrastColor = Colors.White },
            new() { Name = "DarkGrey", Color = Colors.DarkGrey, ContrastColor = Colors.Black },
            new() { Name = "DarkKhaki", Color = Colors.DarkKhaki, ContrastColor = Colors.Black },
            new() { Name = "DarkMagenta", Color = Colors.DarkMagenta, ContrastColor = Colors.White },
            new() { Name = "DarkOliveGreen", Color = Colors.DarkOliveGreen, ContrastColor = Colors.White },
            new() { Name = "DarkOrange", Color = Colors.DarkOrange, ContrastColor = Colors.Black },
            new() { Name = "DarkOrchid", Color = Colors.DarkOrchid, ContrastColor = Colors.White },
            new() { Name = "DarkRed", Color = Colors.DarkRed, ContrastColor = Colors.White },
            new() { Name = "DarkSalmon", Color = Colors.DarkSalmon, ContrastColor = Colors.Black },
            new() { Name = "DarkSeaGreen", Color = Colors.DarkSeaGreen, ContrastColor = Colors.Black },
            new() { Name = "DarkSlateBlue", Color = Colors.DarkSlateBlue, ContrastColor = Colors.White },
            new() { Name = "DarkSlateGray", Color = Colors.DarkSlateGray, ContrastColor = Colors.White },
            new() { Name = "DarkSlateGrey", Color = Colors.DarkSlateGrey, ContrastColor = Colors.White },
            new() { Name = "DarkTurquoise", Color = Colors.DarkTurquoise, ContrastColor = Colors.Black },
            new() { Name = "DarkViolet", Color = Colors.DarkViolet, ContrastColor = Colors.White },
            new() { Name = "DeepPink", Color = Colors.DeepPink, ContrastColor = Colors.White },
            new() { Name = "DeepSkyBlue", Color = Colors.DeepSkyBlue, ContrastColor = Colors.Black },
            new() { Name = "DimGray", Color = Colors.DimGray, ContrastColor = Colors.White },
            new() { Name = "DimGrey", Color = Colors.DimGrey, ContrastColor = Colors.White },
            new() { Name = "DodgerBlue", Color = Colors.DodgerBlue, ContrastColor = Colors.White },
            new() { Name = "Firebrick", Color = Colors.Firebrick, ContrastColor = Colors.White },
            new() { Name = "FloralWhite", Color = Colors.FloralWhite, ContrastColor = Colors.Black },
            new() { Name = "ForestGreen", Color = Colors.ForestGreen, ContrastColor = Colors.White },
            new() { Name = "Fuchsia", Color = Colors.Fuchsia, ContrastColor = Colors.White },
            new() { Name = "Gainsboro", Color = Colors.Gainsboro, ContrastColor = Colors.Black },
            new() { Name = "GhostWhite", Color = Colors.GhostWhite, ContrastColor = Colors.Black },
            new() { Name = "Gold", Color = Colors.Gold, ContrastColor = Colors.Black },
            new() { Name = "Goldenrod", Color = Colors.Goldenrod, ContrastColor = Colors.Black },
            new() { Name = "Gray", Color = Colors.Gray, ContrastColor = Colors.Black },
            new() { Name = "Green", Color = Colors.Green, ContrastColor = Colors.White },
            new() { Name = "GreenYellow", Color = Colors.GreenYellow, ContrastColor = Colors.Black },
            new() { Name = "Grey", Color = Colors.Grey, ContrastColor = Colors.Black },
            new() { Name = "Honeydew", Color = Colors.Honeydew, ContrastColor = Colors.Black },
            new() { Name = "HotPink", Color = Colors.HotPink, ContrastColor = Colors.Black },
            new() { Name = "IndianRed", Color = Colors.IndianRed, ContrastColor = Colors.White },
            new() { Name = "Indigo", Color = Colors.Indigo, ContrastColor = Colors.White },
            new() { Name = "Ivory", Color = Colors.Ivory, ContrastColor = Colors.Black },
            new() { Name = "Khaki", Color = Colors.Khaki, ContrastColor = Colors.Black },
            new() { Name = "Lavender", Color = Colors.Lavender, ContrastColor = Colors.Black },
            new() { Name = "LavenderBlush", Color = Colors.LavenderBlush, ContrastColor = Colors.Black },
            new() { Name = "LawnGreen", Color = Colors.LawnGreen, ContrastColor = Colors.Black },
            new() { Name = "LemonChiffon", Color = Colors.LemonChiffon, ContrastColor = Colors.Black },
            new() { Name = "LightBlue", Color = Colors.LightBlue, ContrastColor = Colors.Black },
            new() { Name = "LightCoral", Color = Colors.LightCoral, ContrastColor = Colors.Black },
            new() { Name = "LightCyan", Color = Colors.LightCyan, ContrastColor = Colors.Black },
            new() { Name = "LightGoldenrodYellow", Color = Colors.LightGoldenrodYellow, ContrastColor = Colors.Black },
            new() { Name = "LightGray", Color = Colors.LightGray, ContrastColor = Colors.Black },
            new() { Name = "LightGreen", Color = Colors.LightGreen, ContrastColor = Colors.Black },
            new() { Name = "LightGrey", Color = Colors.LightGrey, ContrastColor = Colors.Black },
            new() { Name = "LightPink", Color = Colors.LightPink, ContrastColor = Colors.Black },
            new() { Name = "LightSalmon", Color = Colors.LightSalmon, ContrastColor = Colors.Black },
            new() { Name = "LightSeaGreen", Color = Colors.LightSeaGreen, ContrastColor = Colors.Black },
            new() { Name = "LightSkyBlue", Color = Colors.LightSkyBlue, ContrastColor = Colors.Black },
            new() { Name = "LightSlateGray", Color = Colors.LightSlateGray, ContrastColor = Colors.Black },
            new() { Name = "LightSlateGrey", Color = Colors.LightSlateGrey, ContrastColor = Colors.Black },
            new() { Name = "LightSteelBlue", Color = Colors.LightSteelBlue, ContrastColor = Colors.Black },
            new() { Name = "LightYellow", Color = Colors.LightYellow, ContrastColor = Colors.Black },
            new() { Name = "Lime", Color = Colors.Lime, ContrastColor = Colors.Black },
            new() { Name = "LimeGreen", Color = Colors.LimeGreen, ContrastColor = Colors.Black },
            new() { Name = "Linen", Color = Colors.Linen, ContrastColor = Colors.Black },
            new() { Name = "Magenta", Color = Colors.Magenta, ContrastColor = Colors.White },
            new() { Name = "Maroon", Color = Colors.Maroon, ContrastColor = Colors.White },
            new() { Name = "MediumAquamarine", Color = Colors.MediumAquamarine, ContrastColor = Colors.Black },
            new() { Name = "MediumBlue", Color = Colors.MediumBlue, ContrastColor = Colors.White },
            new() { Name = "MediumOrchid", Color = Colors.MediumOrchid, ContrastColor = Colors.Black },
            new() { Name = "MediumPurple", Color = Colors.MediumPurple, ContrastColor = Colors.Black },
            new() { Name = "MediumSeaGreen", Color = Colors.MediumSeaGreen, ContrastColor = Colors.Black },
            new() { Name = "MediumSlateBlue", Color = Colors.MediumSlateBlue, ContrastColor = Colors.White },
            new() { Name = "MediumSpringGreen", Color = Colors.MediumSpringGreen, ContrastColor = Colors.Black },
            new() { Name = "MediumTurquoise", Color = Colors.MediumTurquoise, ContrastColor = Colors.Black },
            new() { Name = "MediumVioletRed", Color = Colors.MediumVioletRed, ContrastColor = Colors.White },
            new() { Name = "MidnightBlue", Color = Colors.MidnightBlue, ContrastColor = Colors.White },
            new() { Name = "MintCream", Color = Colors.MintCream, ContrastColor = Colors.Black },
            new() { Name = "MistyRose", Color = Colors.MistyRose, ContrastColor = Colors.Black },
            new() { Name = "Moccasin", Color = Colors.Moccasin, ContrastColor = Colors.Black },
            new() { Name = "NavajoWhite", Color = Colors.NavajoWhite, ContrastColor = Colors.Black },
            new() { Name = "Navy", Color = Colors.Navy, ContrastColor = Colors.White },
            new() { Name = "OldLace", Color = Colors.OldLace, ContrastColor = Colors.Black },
            new() { Name = "Olive", Color = Colors.Olive, ContrastColor = Colors.White },
            new() { Name = "OliveDrab", Color = Colors.OliveDrab, ContrastColor = Colors.White },
            new() { Name = "Orange", Color = Colors.Orange, ContrastColor = Colors.Black },
            new() { Name = "OrangeRed", Color = Colors.OrangeRed, ContrastColor = Colors.White },
            new() { Name = "Orchid", Color = Colors.Orchid, ContrastColor = Colors.Black },
            new() { Name = "PaleGoldenrod", Color = Colors.PaleGoldenrod, ContrastColor = Colors.Black },
            new() { Name = "PaleGreen", Color = Colors.PaleGreen, ContrastColor = Colors.Black },
            new() { Name = "PaleTurquoise", Color = Colors.PaleTurquoise, ContrastColor = Colors.Black },
            new() { Name = "PaleVioletRed", Color = Colors.PaleVioletRed, ContrastColor = Colors.Black },
            new() { Name = "PapayaWhip", Color = Colors.PapayaWhip, ContrastColor = Colors.Black },
            new() { Name = "PeachPuff", Color = Colors.PeachPuff, ContrastColor = Colors.Black },
            new() { Name = "Peru", Color = Colors.Peru, ContrastColor = Colors.Black },
            new() { Name = "Pink", Color = Colors.Pink, ContrastColor = Colors.Black },
            new() { Name = "Plum", Color = Colors.Plum, ContrastColor = Colors.Black },
            new() { Name = "PowderBlue", Color = Colors.PowderBlue, ContrastColor = Colors.Black },
            new() { Name = "Purple", Color = Colors.Purple, ContrastColor = Colors.White },
            new() { Name = "Red", Color = Colors.Red, ContrastColor = Colors.White },
            new() { Name = "RosyBrown", Color = Colors.RosyBrown, ContrastColor = Colors.Black },
            new() { Name = "RoyalBlue", Color = Colors.RoyalBlue, ContrastColor = Colors.White },
            new() { Name = "SaddleBrown", Color = Colors.SaddleBrown, ContrastColor = Colors.White },
            new() { Name = "Salmon", Color = Colors.Salmon, ContrastColor = Colors.Black },
            new() { Name = "SandyBrown", Color = Colors.SandyBrown, ContrastColor = Colors.Black },
            new() { Name = "SeaGreen", Color = Colors.SeaGreen, ContrastColor = Colors.White },
            new() { Name = "SeaShell", Color = Colors.SeaShell, ContrastColor = Colors.Black },
            new() { Name = "Sienna", Color = Colors.Sienna, ContrastColor = Colors.White },
            new() { Name = "Silver", Color = Colors.Silver, ContrastColor = Colors.Black },
            new() { Name = "SkyBlue", Color = Colors.SkyBlue, ContrastColor = Colors.Black },
            new() { Name = "SlateBlue", Color = Colors.SlateBlue, ContrastColor = Colors.White },
            new() { Name = "SlateGray", Color = Colors.SlateGray, ContrastColor = Colors.White },
            new() { Name = "SlateGrey", Color = Colors.SlateGrey, ContrastColor = Colors.White },
            new() { Name = "Snow", Color = Colors.Snow, ContrastColor = Colors.Black },
            new() { Name = "SpringGreen", Color = Colors.SpringGreen, ContrastColor = Colors.Black },
            new() { Name = "SteelBlue", Color = Colors.SteelBlue, ContrastColor = Colors.White },
            new() { Name = "Tan", Color = Colors.Tan, ContrastColor = Colors.Black },
            new() { Name = "Teal", Color = Colors.Teal, ContrastColor = Colors.White },
            new() { Name = "Thistle", Color = Colors.Thistle, ContrastColor = Colors.Black },
            new() { Name = "Tomato", Color = Colors.Tomato, ContrastColor = Colors.Black },
            new() { Name = "Transparent", Color = Colors.Transparent, ContrastColor = Colors.Black },
            new() { Name = "Turquoise", Color = Colors.Turquoise, ContrastColor = Colors.Black },
            new() { Name = "Violet", Color = Colors.Violet, ContrastColor = Colors.Black },
            new() { Name = "Wheat", Color = Colors.Wheat, ContrastColor = Colors.Black },
            new() { Name = "White", Color = Colors.White, ContrastColor = Colors.Black },
            new() { Name = "WhiteSmoke", Color = Colors.WhiteSmoke, ContrastColor = Colors.Black },
            new() { Name = "Yellow", Color = Colors.Yellow, ContrastColor = Colors.Black },
            new() { Name = "YellowGreen", Color = Colors.YellowGreen, ContrastColor = Colors.Black }
        };

        return new ReadOnlyCollection<ColorOption>(colors);
    }

    public static List<ColorOption> BuildSelectableColors(ReadOnlyCollection<ColorOption> allColors) {
        var selectedColorOptions = new List<ColorOption>();

        foreach (var color in allColors) {
            var alpha = color.Color.GetByteAlpha();
            var green = color.Color.GetByteGreen();
            var red = color.Color.GetByteRed();
            var blue = color.Color.GetByteBlue();

            if ((red == 0 || red == 128 || red == 255) && (green == 0 || green == 128 || green == 255) && (blue == 0 || blue == 128 || blue == 255)) {
                selectedColorOptions.Add(color);
            }
        }

        return selectedColorOptions;
    }

    private static bool IsColorDark(Color color) {
        // Using relative luminance to determine if the color is dark or light
        double brightness = (color.Red * 255 * 299 + color.Green * 255 * 587 + color.Blue * 255 * 114) / 1000;
        return brightness < 128;
    }
}