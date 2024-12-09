using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;

namespace DCCPanelController.Helpers;

public class JsonConverterTrackPiece : JsonConverter<ITrackPiece> {
    public override ITrackPiece? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options) {
        
        // Use the type discriminator or another property to determine the specific type
        using (var document = JsonDocument.ParseValue(ref reader)) {
            if (!document.RootElement.TryGetProperty("TrackObjectType", out var typeProperty)) {
                throw new JsonException("Type property not found.");
            }

            // Switch types based on the `Type` property value
            var typeName = typeProperty.GetString();
            var rawText = document.RootElement.GetRawText();
            ITrackPiece? obj = typeName switch {
                "TrackButton"               => JsonSerializer.Deserialize<TrackButton>(rawText, options),
                "TrackCompass"              => JsonSerializer.Deserialize<TrackCompass>(rawText, options),
                "TrackCorner"               => JsonSerializer.Deserialize<TrackCorner>(rawText, options),
                "TrackCornerContinuation"   => JsonSerializer.Deserialize<TrackCornerContinuation>(rawText, options),
                "TrackCrossing"             => JsonSerializer.Deserialize<TrackCrossing>(rawText, options),
                "TrackImage"                => JsonSerializer.Deserialize<TrackImage>(rawText, options),
                "TrackLabelCircle"          => JsonSerializer.Deserialize<TrackLabelCircle>(rawText, options),
                "TrackLeftTurnout"          => JsonSerializer.Deserialize<TrackLeftTurnout>(rawText, options),
                "TrackPoints"               => JsonSerializer.Deserialize<TrackPoints>(rawText, options),
                "TrackRightTurnout"         => JsonSerializer.Deserialize<TrackRightTurnout>(rawText, options),
                "TrackStraight"             => JsonSerializer.Deserialize<TrackStraight>(rawText, options),
                "TrackStraightContinuation" => JsonSerializer.Deserialize<TrackStraightContinuation>(rawText, options),
                "TrackTerminator"           => JsonSerializer.Deserialize<TrackTerminator>(rawText, options),
                "TrackText"                 => JsonSerializer.Deserialize<TrackText>(rawText, options),
                _                           => throw new JsonException("Unknown type: " + "\"" + typeName + "\""),
            };
            if (obj == null) throw new JsonException("Unknown type: " + "\"" + typeName + "\"" + ".");
            return obj;
        }
    }


    public override void Write(Utf8JsonWriter writer, ITrackPiece value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer,(object)value, options);
    }
}