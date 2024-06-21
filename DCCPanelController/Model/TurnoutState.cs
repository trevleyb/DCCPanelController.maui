using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Turnout with its current state.
/// This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("Id: {Id}, Name: {Name}, State: {State}")]
public partial class TurnoutState : ObservableObject {
    /// <summary>
    /// Represents a Turnout with its current state.
    /// This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public TurnoutState() { }
    
    [JsonPropertyName("Id")]
    [ObservableProperty] 
    private string? _id;
    
    [ObservableProperty] private string? _name;
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;

}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<TurnoutState>))]
internal sealed partial class TurnoutStateContext : JsonSerializerContext {
}

[JsonConverter(typeof(JsonStringEnumConverter<TurnoutStateEnum>))]
public enum TurnoutStateEnum {
    Closed, 
    Thrown, 
    Unknown
} 

