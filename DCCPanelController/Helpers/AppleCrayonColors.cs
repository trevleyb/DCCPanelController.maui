namespace DCCPanelController.Helpers;

public record AppleCrayonColor(string Name, Color Color);

public static class AppleCrayonColors {
    public enum AppleCrayonColorsEnum : uint {
        Licorice  = 0xFF000000,
        Lead      = 0xFF212121,
        Tungsten  = 0xFF424242,
        Iron      = 0xFF5e5e5e,
        Steel     = 0xFF797979,
        Tin       = 0xFF919191,
        Nickel    = 0xFF929292,
        Aluminium = 0xFFA9A9A9,
        Magnesium = 0xFFC0C0C0,
        Silver    = 0xFFD6D6D6,
        Mercury   = 0xFFEBEBEB,
        Snow      = 0xFFFFFFFF,

        Cayenne   = 0xFF941100,
        Mocha     = 0xFF945200,
        Asparagus = 0xFF929000,
        Fern      = 0xFF4F8F00,
        Clover    = 0xFF008f00,
        Moss      = 0xFF009051,
        Teal      = 0xFF009193,
        Ocean     = 0xFF005493,
        Midnight  = 0xFF011993,
        Eggplant  = 0xFF531b93,
        Plum      = 0xFF942193,
        Maroon    = 0xFF941751,

        Maraschino = 0xFFFF2600,
        Tangerine  = 0xFFFF9300,
        Lemon      = 0xFFfffb00,
        Lime       = 0xFF8efa00,
        Spring     = 0xFF00f900,
        SeaFoam    = 0xFF00fa92,
        Turquoise  = 0xFF00fdff,
        Aqua       = 0xFF0096ff,
        Blueberry  = 0xFF0433ff,
        Grape      = 0xFF9437ff,
        Magenta    = 0xFFff40ff,
        Strawberry = 0xFFff2f92,

        Salmon     = 0xFFff7e79,
        Cantaloupe = 0xFFffd479,
        Banana     = 0xFFfffc79,
        Honeydew   = 0xFFd4fb79,
        Flora      = 0xFF73fa79,
        Spindrift  = 0xFF73fcd6,
        Ice        = 0xFF73fdff,
        Sky        = 0xFF76d6ff,
        Orchid     = 0xFF7a81ff,
        Lavender   = 0xFFd783ff,
        Bubblegum  = 0xFFff85ff,
        Carnation  = 0xFFff8ad8,
    }

    // Cache the colors list to avoid recreation
    private static readonly Lazy<IReadOnlyList<Color>>      _colors     = new(() => Crayons.Select(crayon => crayon.Color).ToList());
    private static readonly Lazy<Dictionary<Color, string>> ColorToName = new(() => Crayons.ToDictionary(c => c.Color, c => c.Name));
    private static readonly Lazy<Dictionary<string, Color>> NameToColor = new(() => Crayons.ToDictionary(c => c.Name, c => c.Color, StringComparer.OrdinalIgnoreCase));

    // Cache the crayons list
    private static readonly Lazy<IReadOnlyList<AppleCrayonColor>> _crayons = new(() =>
        new List<AppleCrayonColor> {
            new(nameof(AppleCrayonColorsEnum.Licorice), EnumToColor(AppleCrayonColorsEnum.Licorice)),
            new(nameof(AppleCrayonColorsEnum.Lead), EnumToColor(AppleCrayonColorsEnum.Lead)),
            new(nameof(AppleCrayonColorsEnum.Tungsten), EnumToColor(AppleCrayonColorsEnum.Tungsten)),
            new(nameof(AppleCrayonColorsEnum.Iron), EnumToColor(AppleCrayonColorsEnum.Iron)),
            new(nameof(AppleCrayonColorsEnum.Steel), EnumToColor(AppleCrayonColorsEnum.Steel)),
            new(nameof(AppleCrayonColorsEnum.Tin), EnumToColor(AppleCrayonColorsEnum.Tin)),
            new(nameof(AppleCrayonColorsEnum.Nickel), EnumToColor(AppleCrayonColorsEnum.Nickel)),
            new(nameof(AppleCrayonColorsEnum.Aluminium), EnumToColor(AppleCrayonColorsEnum.Aluminium)),
            new(nameof(AppleCrayonColorsEnum.Magnesium), EnumToColor(AppleCrayonColorsEnum.Magnesium)),
            new(nameof(AppleCrayonColorsEnum.Silver), EnumToColor(AppleCrayonColorsEnum.Silver)),
            new(nameof(AppleCrayonColorsEnum.Mercury), EnumToColor(AppleCrayonColorsEnum.Mercury)),
            new(nameof(AppleCrayonColorsEnum.Snow), EnumToColor(AppleCrayonColorsEnum.Snow)),

            new(nameof(AppleCrayonColorsEnum.Cayenne), EnumToColor(AppleCrayonColorsEnum.Cayenne)),
            new(nameof(AppleCrayonColorsEnum.Mocha), EnumToColor(AppleCrayonColorsEnum.Mocha)),
            new(nameof(AppleCrayonColorsEnum.Asparagus), EnumToColor(AppleCrayonColorsEnum.Asparagus)),
            new(nameof(AppleCrayonColorsEnum.Fern), EnumToColor(AppleCrayonColorsEnum.Fern)),
            new(nameof(AppleCrayonColorsEnum.Clover), EnumToColor(AppleCrayonColorsEnum.Clover)),
            new(nameof(AppleCrayonColorsEnum.Moss), EnumToColor(AppleCrayonColorsEnum.Moss)),
            new(nameof(AppleCrayonColorsEnum.Teal), EnumToColor(AppleCrayonColorsEnum.Teal)),
            new(nameof(AppleCrayonColorsEnum.Ocean), EnumToColor(AppleCrayonColorsEnum.Ocean)),
            new(nameof(AppleCrayonColorsEnum.Midnight), EnumToColor(AppleCrayonColorsEnum.Midnight)),
            new(nameof(AppleCrayonColorsEnum.Eggplant), EnumToColor(AppleCrayonColorsEnum.Eggplant)),
            new(nameof(AppleCrayonColorsEnum.Plum), EnumToColor(AppleCrayonColorsEnum.Plum)),
            new(nameof(AppleCrayonColorsEnum.Maroon), EnumToColor(AppleCrayonColorsEnum.Maroon)),

            new(nameof(AppleCrayonColorsEnum.Maraschino), EnumToColor(AppleCrayonColorsEnum.Maraschino)),
            new(nameof(AppleCrayonColorsEnum.Tangerine), EnumToColor(AppleCrayonColorsEnum.Tangerine)),
            new(nameof(AppleCrayonColorsEnum.Lemon), EnumToColor(AppleCrayonColorsEnum.Lemon)),
            new(nameof(AppleCrayonColorsEnum.Lime), EnumToColor(AppleCrayonColorsEnum.Lime)),
            new(nameof(AppleCrayonColorsEnum.Spring), EnumToColor(AppleCrayonColorsEnum.Spring)),
            new(nameof(AppleCrayonColorsEnum.SeaFoam), EnumToColor(AppleCrayonColorsEnum.SeaFoam)),
            new(nameof(AppleCrayonColorsEnum.Turquoise), EnumToColor(AppleCrayonColorsEnum.Turquoise)),
            new(nameof(AppleCrayonColorsEnum.Aqua), EnumToColor(AppleCrayonColorsEnum.Aqua)),
            new(nameof(AppleCrayonColorsEnum.Blueberry), EnumToColor(AppleCrayonColorsEnum.Blueberry)),
            new(nameof(AppleCrayonColorsEnum.Grape), EnumToColor(AppleCrayonColorsEnum.Grape)),
            new(nameof(AppleCrayonColorsEnum.Magenta), EnumToColor(AppleCrayonColorsEnum.Magenta)),
            new(nameof(AppleCrayonColorsEnum.Strawberry), EnumToColor(AppleCrayonColorsEnum.Strawberry)),

            new(nameof(AppleCrayonColorsEnum.Salmon), EnumToColor(AppleCrayonColorsEnum.Salmon)),
            new(nameof(AppleCrayonColorsEnum.Cantaloupe), EnumToColor(AppleCrayonColorsEnum.Cantaloupe)),
            new(nameof(AppleCrayonColorsEnum.Banana), EnumToColor(AppleCrayonColorsEnum.Banana)),
            new(nameof(AppleCrayonColorsEnum.Honeydew), EnumToColor(AppleCrayonColorsEnum.Honeydew)),
            new(nameof(AppleCrayonColorsEnum.Flora), EnumToColor(AppleCrayonColorsEnum.Flora)),
            new(nameof(AppleCrayonColorsEnum.Spindrift), EnumToColor(AppleCrayonColorsEnum.Spindrift)),
            new(nameof(AppleCrayonColorsEnum.Ice), EnumToColor(AppleCrayonColorsEnum.Ice)),
            new(nameof(AppleCrayonColorsEnum.Sky), EnumToColor(AppleCrayonColorsEnum.Sky)),
            new(nameof(AppleCrayonColorsEnum.Orchid), EnumToColor(AppleCrayonColorsEnum.Orchid)),
            new(nameof(AppleCrayonColorsEnum.Lavender), EnumToColor(AppleCrayonColorsEnum.Lavender)),
            new(nameof(AppleCrayonColorsEnum.Bubblegum), EnumToColor(AppleCrayonColorsEnum.Bubblegum)),
            new(nameof(AppleCrayonColorsEnum.Carnation), EnumToColor(AppleCrayonColorsEnum.Carnation)),
        });

    public static IReadOnlyList<Color> Colors => _colors.Value;
    public static IReadOnlyList<AppleCrayonColor> Crayons => _crayons.Value;
    public static IReadOnlyList<AppleCrayonColor> Lazy => _crayons.Value;

    public static string Name(Color color) => ColorToName.Value.GetValueOrDefault(color, "Unknown");

    public static Color Value(string name) => NameToColor.Value.TryGetValue(name, out var color) ? color : Microsoft.Maui.Graphics.Colors.White;

    public static bool IsColorLight(Color color) {
        var luminance = 0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue;
        return luminance > 0.5;
    }

    public static Color GetContrastingTextColor(Color backgroundColor) {
        var luminance = 0.299 * backgroundColor.Red + 0.587 * backgroundColor.Green + 0.114 * backgroundColor.Blue;
        return luminance > 0.5 ? EnumToColor(AppleCrayonColorsEnum.Licorice) : EnumToColor(AppleCrayonColorsEnum.Snow);
    }

    public static Color EnumToColor(AppleCrayonColorsEnum color) => Color.FromArgb($"{(uint)color:X8}");
}