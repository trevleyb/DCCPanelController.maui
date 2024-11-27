using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Turnout with its current state.
/// This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("Id: {Id}, SystemName: {Name}, State: {State}")]
public partial class Turnout : ObservableObject {
    /// <summary>
    /// Represents a Turnout with its current state.
    /// This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Turnout() { }

    [JsonPropertyName("Id")] 
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private TurnoutStateEnum _state   = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _default = TurnoutStateEnum.Unknown;
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<Turnout>))]
internal sealed partial class TurnoutContext : JsonSerializerContext { }

[JsonConverter(typeof(JsonStringEnumConverter<TurnoutStateEnum>))]
public enum TurnoutStateEnum {
    Closed,
    Thrown,
    Unknown
}