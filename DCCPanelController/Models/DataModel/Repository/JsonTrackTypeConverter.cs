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

    public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options) {
        JsonSerializer.Serialize<object>(writer, value, options);
    }
}


            // Entity? obj = typeName switch {
            //     "ButtonEntity"               => JsonSerializer.Deserialize<ButtonEntity>(rawText, options),
            //     "CircleEntity"               => JsonSerializer.Deserialize<CircleEntity>(rawText, options),
            //     "CircleLabelEntity"          => JsonSerializer.Deserialize<CircleLabelEntity>(rawText, options),
            //     "CompassEntity"              => JsonSerializer.Deserialize<CompassEntity>(rawText, options),
            //     "CornerContinuationEntity"   => JsonSerializer.Deserialize<CornerContinuationEntity>(rawText, options),
            //     "CornerEntity"               => JsonSerializer.Deserialize<CornerEntity>(rawText, options),
            //     "CrossingEntity"             => JsonSerializer.Deserialize<CrossingEntity>(rawText, options),
            //     "ImageEntity"                => JsonSerializer.Deserialize<ImageEntity>(rawText, options),
            //     "LeftTurnoutEntity"          => JsonSerializer.Deserialize<LeftTurnoutEntity>(rawText, options),
            //     "LineEntity"                 => JsonSerializer.Deserialize<LineEntity>(rawText, options),
            //     "PointsEntity"               => JsonSerializer.Deserialize<PointsEntity>(rawText, options),
            //     "RectangleEntity"            => JsonSerializer.Deserialize<RectangleEntity>(rawText, options),
            //     "RightTurnoutEntity"         => JsonSerializer.Deserialize<RightTurnoutEntity>(rawText, options),
            //     "StraightContinuationEntity" => JsonSerializer.Deserialize<StraightContinuationEntity>(rawText, options),
            //     "StraightEntity"             => JsonSerializer.Deserialize<StraightEntity>(rawText, options),
            //     "TerminatorEntity"           => JsonSerializer.Deserialize<TerminatorEntity>(rawText, options),
            //     "TextEntity"                 => JsonSerializer.Deserialize<TextEntity>(rawText, options),
            //     _                            => throw new JsonException("Unknown type: " + "\"" + typeName + "\"")
            // };
