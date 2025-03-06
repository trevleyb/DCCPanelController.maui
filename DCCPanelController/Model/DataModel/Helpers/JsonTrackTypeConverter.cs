using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Model.DataModel.Tracks;

namespace DCCPanelController.Model.DataModel.Helpers;

public class JsonTrackTypeConverter : JsonConverter<Track?> {
    public override Track? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options) {
        // Use the type discriminator or another property to determine the specific type
        // --------------------------------------------------------------------------------------------
        using (var document = JsonDocument.ParseValue(ref reader)) {
            var root = document.RootElement;

            if (!root.TryGetProperty("TrackType", out var typeProperty)) {
                throw new JsonException($"Type property not found: {root.ToString()}");
            }

            // Switch types based on the `Type` property value
            // --------------------------------------------------------------------------------------------
            var rawText = root.GetRawText();
            var typeName = typeProperty.GetString();

            Track? obj = typeName switch {
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
                "TrackCircle"               => JsonSerializer.Deserialize<TrackCircle>(rawText, options),
                "TrackRectangle"            => JsonSerializer.Deserialize<TrackRectangle>(rawText, options),
                "TrackLine"                 => JsonSerializer.Deserialize<TrackLine>(rawText, options),
                _                           => throw new JsonException("Unknown type: " + "\"" + typeName + "\"")
            };

            if (obj == null) {
                Debug.WriteLine("Unknown type JSON type: " + "\"" + typeName + "\"" + ". Skipped.");
            }
            return obj;
        }
    }

    public override void Write(Utf8JsonWriter writer, Track value, JsonSerializerOptions options) {
        JsonSerializer.Serialize<object>(writer, value, options);
    }
}