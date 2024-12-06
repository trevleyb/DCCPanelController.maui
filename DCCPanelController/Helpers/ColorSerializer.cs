using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Helpers;

public class ColorSerializer {
    [JsonConverter(typeof(MauiColorJsonConverter))]
    public Color MyColor { get; set; }
}

public class MauiColorJsonConverter : JsonConverter<Color> {
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var colorString = reader.GetString();
        return Color.FromArgb(colorString) ?? Colors.White;
    }

    public override void Write(Utf8JsonWriter writer, Color colorValue, JsonSerializerOptions options) {
        var hex = colorValue?.ToHex() ?? Colors.White.ToHex();
        writer.WriteStringValue(hex);
    }
}