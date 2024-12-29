using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("Id: {Id}, SystemName: {Name}, State: {State}")]
public partial class Turnout : ObservableObject {

    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private int? _dccAddress;
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _default = TurnoutStateEnum.Unknown;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Turnout() { }
}