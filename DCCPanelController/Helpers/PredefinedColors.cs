namespace DCCPanelController.Helpers;

public class ColorOption {
    public string Name { get; set; }
    public Color Color { get; set; }
    public Color ContrastColor { get; set; }
}

public static class PredefinedColors {

	private static readonly object padlock = new object();
	private static List<ColorOption>? colorOptions = null;

	public static List<ColorOption> GetColors() {
		if (colorOptions == null) {
			lock (padlock) {
				if (colorOptions == null) {
					colorOptions = BuildColorOptions();
				}
			}
		}
		return colorOptions;
	}

	private static List<ColorOption> BuildColorOptions() {

		var colors = new List<ColorOption>() {
			new ColorOption { Name = "AliceBlue", Color = Colors.AliceBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "AntiqueWhite", Color = Colors.AntiqueWhite, ContrastColor = Colors.Black },
			new ColorOption { Name = "Aqua", Color = Colors.Aqua, ContrastColor = Colors.Black },
			new ColorOption { Name = "Aquamarine", Color = Colors.Aquamarine, ContrastColor = Colors.Black },
			new ColorOption { Name = "Azure", Color = Colors.Azure, ContrastColor = Colors.Black },
			new ColorOption { Name = "Beige", Color = Colors.Beige, ContrastColor = Colors.Black },
			new ColorOption { Name = "Bisque", Color = Colors.Bisque, ContrastColor = Colors.Black },
			new ColorOption { Name = "Black", Color = Colors.Black, ContrastColor = Colors.White },
			new ColorOption { Name = "BlanchedAlmond", Color = Colors.BlanchedAlmond, ContrastColor = Colors.Black },
			new ColorOption { Name = "Blue", Color = Colors.Blue, ContrastColor = Colors.White },
			new ColorOption { Name = "BlueViolet", Color = Colors.BlueViolet, ContrastColor = Colors.White },
			new ColorOption { Name = "Brown", Color = Colors.Brown, ContrastColor = Colors.White },
			new ColorOption { Name = "BurlyWood", Color = Colors.BurlyWood, ContrastColor = Colors.Black },
			new ColorOption { Name = "CadetBlue", Color = Colors.CadetBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "Chartreuse", Color = Colors.Chartreuse, ContrastColor = Colors.Black },
			new ColorOption { Name = "Chocolate", Color = Colors.Chocolate, ContrastColor = Colors.White },
			new ColorOption { Name = "Coral", Color = Colors.Coral, ContrastColor = Colors.Black },
			new ColorOption { Name = "CornflowerBlue", Color = Colors.CornflowerBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "Cornsilk", Color = Colors.Cornsilk, ContrastColor = Colors.Black },
			new ColorOption { Name = "Crimson", Color = Colors.Crimson, ContrastColor = Colors.White },
			new ColorOption { Name = "Cyan", Color = Colors.Cyan, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkBlue", Color = Colors.DarkBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkCyan", Color = Colors.DarkCyan, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkGoldenrod", Color = Colors.DarkGoldenrod, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkGray", Color = Colors.DarkGray, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkGreen", Color = Colors.DarkGreen, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkGrey", Color = Colors.DarkGrey, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkKhaki", Color = Colors.DarkKhaki, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkMagenta", Color = Colors.DarkMagenta, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkOliveGreen", Color = Colors.DarkOliveGreen, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkOrange", Color = Colors.DarkOrange, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkOrchid", Color = Colors.DarkOrchid, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkRed", Color = Colors.DarkRed, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkSalmon", Color = Colors.DarkSalmon, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkSeaGreen", Color = Colors.DarkSeaGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkSlateBlue", Color = Colors.DarkSlateBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkSlateGray", Color = Colors.DarkSlateGray, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkSlateGrey", Color = Colors.DarkSlateGrey, ContrastColor = Colors.White },
			new ColorOption { Name = "DarkTurquoise", Color = Colors.DarkTurquoise, ContrastColor = Colors.Black },
			new ColorOption { Name = "DarkViolet", Color = Colors.DarkViolet, ContrastColor = Colors.White },
			new ColorOption { Name = "DeepPink", Color = Colors.DeepPink, ContrastColor = Colors.White },
			new ColorOption { Name = "DeepSkyBlue", Color = Colors.DeepSkyBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "DimGray", Color = Colors.DimGray, ContrastColor = Colors.White },
			new ColorOption { Name = "DimGrey", Color = Colors.DimGrey, ContrastColor = Colors.White },
			new ColorOption { Name = "DodgerBlue", Color = Colors.DodgerBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "Firebrick", Color = Colors.Firebrick, ContrastColor = Colors.White },
			new ColorOption { Name = "FloralWhite", Color = Colors.FloralWhite, ContrastColor = Colors.Black },
			new ColorOption { Name = "ForestGreen", Color = Colors.ForestGreen, ContrastColor = Colors.White },
			new ColorOption { Name = "Fuchsia", Color = Colors.Fuchsia, ContrastColor = Colors.White },
			new ColorOption { Name = "Gainsboro", Color = Colors.Gainsboro, ContrastColor = Colors.Black },
			new ColorOption { Name = "GhostWhite", Color = Colors.GhostWhite, ContrastColor = Colors.Black },
			new ColorOption { Name = "Gold", Color = Colors.Gold, ContrastColor = Colors.Black },
			new ColorOption { Name = "Goldenrod", Color = Colors.Goldenrod, ContrastColor = Colors.Black },
			new ColorOption { Name = "Gray", Color = Colors.Gray, ContrastColor = Colors.Black },
			new ColorOption { Name = "Green", Color = Colors.Green, ContrastColor = Colors.White },
			new ColorOption { Name = "GreenYellow", Color = Colors.GreenYellow, ContrastColor = Colors.Black },
			new ColorOption { Name = "Grey", Color = Colors.Grey, ContrastColor = Colors.Black },
			new ColorOption { Name = "Honeydew", Color = Colors.Honeydew, ContrastColor = Colors.Black },
			new ColorOption { Name = "HotPink", Color = Colors.HotPink, ContrastColor = Colors.Black },
			new ColorOption { Name = "IndianRed", Color = Colors.IndianRed, ContrastColor = Colors.White },
			new ColorOption { Name = "Indigo", Color = Colors.Indigo, ContrastColor = Colors.White },
			new ColorOption { Name = "Ivory", Color = Colors.Ivory, ContrastColor = Colors.Black },
			new ColorOption { Name = "Khaki", Color = Colors.Khaki, ContrastColor = Colors.Black },
			new ColorOption { Name = "Lavender", Color = Colors.Lavender, ContrastColor = Colors.Black },
			new ColorOption { Name = "LavenderBlush", Color = Colors.LavenderBlush, ContrastColor = Colors.Black },
			new ColorOption { Name = "LawnGreen", Color = Colors.LawnGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "LemonChiffon", Color = Colors.LemonChiffon, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightBlue", Color = Colors.LightBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightCoral", Color = Colors.LightCoral, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightCyan", Color = Colors.LightCyan, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightGoldenrodYellow", Color = Colors.LightGoldenrodYellow, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightGray", Color = Colors.LightGray, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightGreen", Color = Colors.LightGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightGrey", Color = Colors.LightGrey, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightPink", Color = Colors.LightPink, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSalmon", Color = Colors.LightSalmon, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSeaGreen", Color = Colors.LightSeaGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSkyBlue", Color = Colors.LightSkyBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSlateGray", Color = Colors.LightSlateGray, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSlateGrey", Color = Colors.LightSlateGrey, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightSteelBlue", Color = Colors.LightSteelBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "LightYellow", Color = Colors.LightYellow, ContrastColor = Colors.Black },
			new ColorOption { Name = "Lime", Color = Colors.Lime, ContrastColor = Colors.Black },
			new ColorOption { Name = "LimeGreen", Color = Colors.LimeGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "Linen", Color = Colors.Linen, ContrastColor = Colors.Black },
			new ColorOption { Name = "Magenta", Color = Colors.Magenta, ContrastColor = Colors.White },
			new ColorOption { Name = "Maroon", Color = Colors.Maroon, ContrastColor = Colors.White },
			new ColorOption { Name = "MediumAquamarine", Color = Colors.MediumAquamarine, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumBlue", Color = Colors.MediumBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "MediumOrchid", Color = Colors.MediumOrchid, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumPurple", Color = Colors.MediumPurple, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumSeaGreen", Color = Colors.MediumSeaGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumSlateBlue", Color = Colors.MediumSlateBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "MediumSpringGreen", Color = Colors.MediumSpringGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumTurquoise", Color = Colors.MediumTurquoise, ContrastColor = Colors.Black },
			new ColorOption { Name = "MediumVioletRed", Color = Colors.MediumVioletRed, ContrastColor = Colors.White },
			new ColorOption { Name = "MidnightBlue", Color = Colors.MidnightBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "MintCream", Color = Colors.MintCream, ContrastColor = Colors.Black },
			new ColorOption { Name = "MistyRose", Color = Colors.MistyRose, ContrastColor = Colors.Black },
			new ColorOption { Name = "Moccasin", Color = Colors.Moccasin, ContrastColor = Colors.Black },
			new ColorOption { Name = "NavajoWhite", Color = Colors.NavajoWhite, ContrastColor = Colors.Black },
			new ColorOption { Name = "Navy", Color = Colors.Navy, ContrastColor = Colors.White },
			new ColorOption { Name = "OldLace", Color = Colors.OldLace, ContrastColor = Colors.Black },
			new ColorOption { Name = "Olive", Color = Colors.Olive, ContrastColor = Colors.White },
			new ColorOption { Name = "OliveDrab", Color = Colors.OliveDrab, ContrastColor = Colors.White },
			new ColorOption { Name = "Orange", Color = Colors.Orange, ContrastColor = Colors.Black },
			new ColorOption { Name = "OrangeRed", Color = Colors.OrangeRed, ContrastColor = Colors.White },
			new ColorOption { Name = "Orchid", Color = Colors.Orchid, ContrastColor = Colors.Black },
			new ColorOption { Name = "PaleGoldenrod", Color = Colors.PaleGoldenrod, ContrastColor = Colors.Black },
			new ColorOption { Name = "PaleGreen", Color = Colors.PaleGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "PaleTurquoise", Color = Colors.PaleTurquoise, ContrastColor = Colors.Black },
			new ColorOption { Name = "PaleVioletRed", Color = Colors.PaleVioletRed, ContrastColor = Colors.Black },
			new ColorOption { Name = "PapayaWhip", Color = Colors.PapayaWhip, ContrastColor = Colors.Black },
			new ColorOption { Name = "PeachPuff", Color = Colors.PeachPuff, ContrastColor = Colors.Black },
			new ColorOption { Name = "Peru", Color = Colors.Peru, ContrastColor = Colors.Black },
			new ColorOption { Name = "Pink", Color = Colors.Pink, ContrastColor = Colors.Black },
			new ColorOption { Name = "Plum", Color = Colors.Plum, ContrastColor = Colors.Black },
			new ColorOption { Name = "PowderBlue", Color = Colors.PowderBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "Purple", Color = Colors.Purple, ContrastColor = Colors.White },
			new ColorOption { Name = "Red", Color = Colors.Red, ContrastColor = Colors.White },
			new ColorOption { Name = "RosyBrown", Color = Colors.RosyBrown, ContrastColor = Colors.Black },
			new ColorOption { Name = "RoyalBlue", Color = Colors.RoyalBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "SaddleBrown", Color = Colors.SaddleBrown, ContrastColor = Colors.White },
			new ColorOption { Name = "Salmon", Color = Colors.Salmon, ContrastColor = Colors.Black },
			new ColorOption { Name = "SandyBrown", Color = Colors.SandyBrown, ContrastColor = Colors.Black },
			new ColorOption { Name = "SeaGreen", Color = Colors.SeaGreen, ContrastColor = Colors.White },
			new ColorOption { Name = "SeaShell", Color = Colors.SeaShell, ContrastColor = Colors.Black },
			new ColorOption { Name = "Sienna", Color = Colors.Sienna, ContrastColor = Colors.White },
			new ColorOption { Name = "Silver", Color = Colors.Silver, ContrastColor = Colors.Black },
			new ColorOption { Name = "SkyBlue", Color = Colors.SkyBlue, ContrastColor = Colors.Black },
			new ColorOption { Name = "SlateBlue", Color = Colors.SlateBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "SlateGray", Color = Colors.SlateGray, ContrastColor = Colors.White },
			new ColorOption { Name = "SlateGrey", Color = Colors.SlateGrey, ContrastColor = Colors.White },
			new ColorOption { Name = "Snow", Color = Colors.Snow, ContrastColor = Colors.Black },
			new ColorOption { Name = "SpringGreen", Color = Colors.SpringGreen, ContrastColor = Colors.Black },
			new ColorOption { Name = "SteelBlue", Color = Colors.SteelBlue, ContrastColor = Colors.White },
			new ColorOption { Name = "Tan", Color = Colors.Tan, ContrastColor = Colors.Black },
			new ColorOption { Name = "Teal", Color = Colors.Teal, ContrastColor = Colors.White },
			new ColorOption { Name = "Thistle", Color = Colors.Thistle, ContrastColor = Colors.Black },
			new ColorOption { Name = "Tomato", Color = Colors.Tomato, ContrastColor = Colors.Black },
			new ColorOption { Name = "Transparent", Color = Colors.Transparent, ContrastColor = Colors.Black },
			new ColorOption { Name = "Turquoise", Color = Colors.Turquoise, ContrastColor = Colors.Black },
			new ColorOption { Name = "Violet", Color = Colors.Violet, ContrastColor = Colors.Black },
			new ColorOption { Name = "Wheat", Color = Colors.Wheat, ContrastColor = Colors.Black },
			new ColorOption { Name = "White", Color = Colors.White, ContrastColor = Colors.Black },
			new ColorOption { Name = "WhiteSmoke", Color = Colors.WhiteSmoke, ContrastColor = Colors.Black },
			new ColorOption { Name = "Yellow", Color = Colors.Yellow, ContrastColor = Colors.Black },
			new ColorOption { Name = "YellowGreen", Color = Colors.YellowGreen, ContrastColor = Colors.Black },
		};
		return colors.Where(color => color.Color.Green == 0 || color.Color.Red == 0 || color.Color.Blue == 0 || color.Color.Green == 255 || color.Color.Red == 255 || color.Color.Blue == 255).ToList();
	}

	private static bool IsColorDark(Color color) {
        // Using relative luminance to determine if the color is dark or light
        double brightness = ((color.Red * 255 * 299) +
                             (color.Green * 255 * 587) +
                             (color.Blue * 255 * 114)) / 1000;

        return brightness < 128;
    }

    private static Color GetContrastingColor(Color color) {
        return IsColorDark(color) ? Colors.White : Colors.Black;
    }

}

