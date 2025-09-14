using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCCPanelController.Models.DataModel.Repository;

public class ExpressionFilterConverter<T> : JsonConverter<List<T>> {
    private readonly Func<T, bool> _predicate;

    public ExpressionFilterConverter(Expression<Func<T, bool>> expression) => _predicate = expression.Compile();

    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options) {
        var filteredList = value?.Where(_predicate).ToList() ?? new List<T>();
        JsonSerializer.Serialize(writer, filteredList, options);
    }
}