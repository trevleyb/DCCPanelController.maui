using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Repository;

public class JsonTrackTypeConverter : JsonConverter<Entity> {
    public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options) {
        // Use the type discriminator or another property to determine the specific type
        // --------------------------------------------------------------------------------------------
        using (var document = JsonDocument.ParseValue(ref reader)) {
            var root = document.RootElement;

            if (!root.TryGetProperty("Type", out var typeProperty)) {
                throw new JsonException($"Type property not found: {root.ToString()}");
            }

            // Switch types based on the `Type` property value
            // --------------------------------------------------------------------------------------------
            var rawText = root.GetRawText();
            var typeName = typeProperty.GetString();

            var obj = Type.GetType($"DCCPanelController.Models.DataModel.Entities.{typeName}") is { } entityType
                ? (Entity?)JsonSerializer.Deserialize(rawText, entityType, options)
                : throw new JsonException("Unknown type: " + "\"" + typeName + "\"");

            if (obj == null) throw new ApplicationException("Unknown type JSON type: " + "\"" + typeName + "\"" + ". Skipped.");
            return obj;
        }
    }

    public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options) => JsonSerializer.Serialize<object>(writer, value, options);
}