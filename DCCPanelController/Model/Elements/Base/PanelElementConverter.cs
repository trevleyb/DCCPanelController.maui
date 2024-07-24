using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCCPanelController.Model.Elements.Base;

public class PanelElementConverter : JsonConverter<IPanelElement>
{
    private readonly Dictionary<string, Type> _typeMapping = new Dictionary<string, Type>
    {
        { nameof(TextPanelElement),  typeof(TextPanelElement) },
        { nameof(TrackPanelElement), typeof(TrackPanelElement) },
        { nameof(TurnoutPanelElement), typeof(TurnoutPanelElement) },
        { nameof(RoutePanelElement), typeof(RoutePanelElement) },
        { nameof(ImagePanelElement), typeof(ImagePanelElement) },
        { nameof(ButtonPanelElement), typeof(ButtonPanelElement) },
        { nameof(CircleTextPanelElement), typeof(CircleTextPanelElement) }
        // Add other mappings here
    };

    public override IPanelElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader)) {
            if (doc.RootElement.TryGetProperty(nameof(IPanelElement.ElementType), out JsonElement typeElement)) {
                var typeName = typeElement.GetString();
                if (typeName != null && _typeMapping.TryGetValue(typeName, out var type)) {
                    return (IPanelElement)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options);
                }
            }
            throw new JsonException($"Unknown type for IPanelElement: {typeToConvert.ToString()}");
        }
    }

    public override void Write(Utf8JsonWriter writer, IPanelElement value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}