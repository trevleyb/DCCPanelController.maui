using System.Text.Json;
using System.Text.Json.Serialization;
using Color = Microsoft.Maui.Graphics.Color;

namespace DCCPanelController.Models.DataModel.Repository;

public class MauiColorJsonConverter : JsonConverter<Color> {
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        try {
            var colorString = reader.GetString();
            return Color.FromArgb(colorString) ?? Colors.White;
        } catch {
            return Colors.White;
        }
    }

    public override void Write(Utf8JsonWriter writer, Color colorValue, JsonSerializerOptions options) {
        var hex = colorValue?.ToArgbHex() ?? Colors.White.ToArgbHex();
        writer.WriteStringValue(hex);
    }
}