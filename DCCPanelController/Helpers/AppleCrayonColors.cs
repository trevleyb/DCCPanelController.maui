using CommunityToolkit.Maui.Core.Extensions;

namespace DCCPanelController.Helpers;

public static class AppleCrayonColors {
    public record AppleCrayonColor(string Name, Color Color);

    public static string Name(Color color) => Crayons.FirstOrDefault(crayon => Equals(crayon.Color, color))?.Name ?? "Unknown";
    public static Color Value(string name) => Crayons.FirstOrDefault(crayon => crayon.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Color ?? Microsoft.Maui.Graphics.Colors.White;
    public static IReadOnlyList<Color> Colors => Crayons.Select(crayon => crayon.Color).ToList();    

    public static IReadOnlyList<AppleCrayonColor> Crayons => new List<AppleCrayonColor>() {
        { new("Licorice", Color.FromArgb("FF000000")) },
        { new("Lead", Color.FromArgb("FF212121")) },
        { new("Tungsten", Color.FromArgb("FF424242")) },
        { new("Iron", Color.FromArgb("FF5e5e5e")) },
        { new("Steel", Color.FromArgb("FF797979")) },
        { new("Tin", Color.FromArgb("FF919191")) },
        { new("Nickel", Color.FromArgb("FF929292")) },
        { new("Aluminium", Color.FromArgb("FFA9A9A9")) },
        { new("Magnesium", Color.FromArgb("FFC0C0C0")) },
        { new("Silver", Color.FromArgb("FFD6D6D6")) },
        { new("Mercury", Color.FromArgb("FFEBEBEB")) },
        { new("Snow", Color.FromArgb("FFFFFFFF")) },

        { new("Cayenne", Color.FromArgb("FF941100")) },
        { new("Mocha", Color.FromArgb("FF945200")) },
        { new("Asparagus", Color.FromArgb("FF929000")) },
        { new("Fern", Color.FromArgb("FF4F8F00")) },
        { new("Clover", Color.FromArgb("FF008f00")) },
        { new("Moss", Color.FromArgb("FF009051")) },
        { new("Teal", Color.FromArgb("FF009193")) },
        { new("Ocean", Color.FromArgb("FF005493")) },
        { new("Midnight", Color.FromArgb("FF011993")) },
        { new("Eggplant", Color.FromArgb("FF531b93")) },
        { new("Plum", Color.FromArgb("FF942193")) },
        { new("Maroon", Color.FromArgb("FF941751")) },

        { new("Maraschino", Color.FromArgb("FFFF2600")) },
        { new("Tangerine", Color.FromArgb("FFFF9300")) },
        { new("Lemon", Color.FromArgb("FFfffb00")) },
        { new("Lime", Color.FromArgb("FF8efa00")) },
        { new("Spring", Color.FromArgb("FF00f900")) },
        { new("Sea Foam", Color.FromArgb("FF00fa92")) },
        { new("Turquoise", Color.FromArgb("FF00fdff")) },
        { new("Aqua", Color.FromArgb("FF0096ff")) },
        { new("Blueberry", Color.FromArgb("FF0433ff")) },
        { new("Grape", Color.FromArgb("FF9437ff")) },
        { new("Magenta", Color.FromArgb("FFff40ff")) },
        { new("Strawberry", Color.FromArgb("FFff2f92")) },

        { new("Salmon", Color.FromArgb("FFff7e79")) },
        { new("Cantaloupe", Color.FromArgb("FFffd479")) },
        { new("Banana", Color.FromArgb("FFfffc79")) },
        { new("Honeydew", Color.FromArgb("FFd4fb79")) },
        { new("Flora", Color.FromArgb("FF73fa79")) },
        { new("Spindrift", Color.FromArgb("FF73fcd6")) },
        { new("Ice", Color.FromArgb("FF73fdff")) },
        { new("Sky", Color.FromArgb("FF76d6ff")) },
        { new("Orchid", Color.FromArgb("FF7a81ff")) },
        { new("Lavender", Color.FromArgb("FFd783ff")) },
        { new("Bubblegum", Color.FromArgb("FFff85ff")) },
        { new("Carnation", Color.FromArgb("FFff8ad8")) },
    };
}