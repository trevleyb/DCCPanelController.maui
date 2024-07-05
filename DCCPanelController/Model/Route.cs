using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

public partial class Route : ObservableObject {
    /// <summary>
    /// Represents a Turnout with its current state.
    /// This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }
    
    [JsonPropertyName("Id")]
    [ObservableProperty] 
    private string? _id;
    
    [ObservableProperty] private string? _name;
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<Route>))]
internal sealed partial class RouteContext : JsonSerializerContext {
}

[JsonConverter(typeof(JsonStringEnumConverter<RouteStateEnum>))]
public enum RouteStateEnum {
    Active, 
    Inactive, 
    Unknown
} 

