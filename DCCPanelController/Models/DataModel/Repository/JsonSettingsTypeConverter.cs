using System.Text.Json;
using System.Text.Json.Serialization;
using DCCPanelController.Clients;
using DCCPanelController.Clients.Jmri;
using DCCPanelController.Clients.Simulator;
using DCCPanelController.Clients.WiThrottle;

namespace DCCPanelController.Models.DataModel.Repository;

public class JsonSettingsTypeConverter : JsonConverter<IDccClientSettings> {
    public override IDccClientSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options) {
        // Use the type discriminator or another property to determine the specific type
        // --------------------------------------------------------------------------------------------
        using (var document = JsonDocument.ParseValue(ref reader)) {
            var root = document.RootElement;
            if (!root.TryGetProperty("Type", out var typeProperty)) {
                throw new JsonException($"Name property not found: {root.ToString()}");
            }

            // Switch types based on the `Type` property value
            // --------------------------------------------------------------------------------------------
            var rawText = root.GetRawText();
            var typeName = typeProperty.GetString();
            IDccClientSettings? obj = typeName?.ToLowerInvariant() switch {
                "withrottle" => JsonSerializer.Deserialize<WiThrottleSettings>(rawText, options),
                "jmri"       => JsonSerializer.Deserialize<JmriSettings>(rawText, options),
                "simulator"  => JsonSerializer.Deserialize<SimulatorSettings>(rawText, options),
                _            => throw new ArgumentOutOfRangeException($"Invalid Settings Type detected: {typeName}"),
            };
            if (obj == null) throw new ApplicationException("Unknown type JSON type: " + "\"" + typeName + "\"" + ". Skipped.");
            return obj;
        }
    }

    public override void Write(Utf8JsonWriter writer, IDccClientSettings value, JsonSerializerOptions options) => JsonSerializer.Serialize<object>(writer, value, options);
}