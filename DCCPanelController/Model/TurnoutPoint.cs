using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Turnout with its current state.
/// This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("Id: {Id}, SystemName: {Name}, State: {State}")]
public partial class TurnoutPoint : ObservableObject {
    /// <summary>
    /// Represents a Turnout with its current state.
    /// This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public TurnoutPoint() { }

    [JsonPropertyName("Id")] [ObservableProperty]
    private string? _id;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    [ObservableProperty] private int _sortOrder = 0;
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<TurnoutPoint>))]
internal sealed partial class TurnoutPointContext : JsonSerializerContext { }