using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCCPanelController.Helpers;

public class JsonConverterEnumToString<T> : JsonConverter<T> where T : struct, Enum {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
        case JsonTokenType.String: {
            // If it's a string, attempt to parse it as an enum name
            var enumString = reader.GetString();
            if (Enum.TryParse(enumString, true, out T value)) {
                return value;
            }

            throw new JsonException($"Unable to convert \"{enumString}\" to Enum \"{typeof(T)}\".");
        }
        case JsonTokenType.Number: {
            // If it's a number, convert it to the enum value
            var enumValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(T), enumValue)) {
                return (T)(object)enumValue;
            }

            throw new JsonException($"Value \"{enumValue}\" is not valid for Enum \"{typeof(T)}\".");
        }
        default:
            throw new JsonException($"Unexpected token {reader.TokenType} when parsing Enum \"{typeof(T)}\".");
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        // Write the enum as a string to JSON
        writer.WriteStringValue(value.ToString());
    }
}