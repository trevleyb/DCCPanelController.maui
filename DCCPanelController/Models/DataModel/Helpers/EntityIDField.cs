using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Helpers;

[JsonConverter(typeof(EntityIDFieldJsonConverter))]
public class EntityIDField(string value) : IEquatable<EntityIDField>, IEquatable<string> {
    public string Value { get; } = value ?? string.Empty;

    // Implicit conversions - these make it work "like a string"
    public static implicit operator string(EntityIDField id) => id?.Value ?? string.Empty;
    public static implicit operator EntityIDField(string value) => new EntityIDField(value);

    // String comparison methods
    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj switch {
        EntityIDField other => Value == other.Value,
        string str          => Value == str,
        _                   => false
    };

    public bool Equals(EntityIDField? other) => other != null && Value == other.Value;
    public bool Equals(string? other) => Value == other;

    public override int GetHashCode() => Value.GetHashCode();

    // String methods for seamless usage
    public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(Value);
    public static bool IsNullOrWhiteSpace(EntityIDField id) => string.IsNullOrWhiteSpace(id?.Value);
}

public class EntityIDFieldJsonConverter : JsonConverter<EntityIDField> {
    public override EntityIDField? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        return value == null ? null : new EntityIDField(value);
    }

    public override void Write(Utf8JsonWriter writer, EntityIDField? value, JsonSerializerOptions options) {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value);
    }
}