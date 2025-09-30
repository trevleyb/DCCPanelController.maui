using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Turnout : DccTable {
    [ObservableProperty] private TurnoutStateEnum _state   = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _default = TurnoutStateEnum.Unknown;

    public event EventHandler<TurnoutStateEnum>? StateChanged;
    partial void OnStateChanged(TurnoutStateEnum value) => StateChanged?.Invoke(this, value);
   
    public void ToggleState() {
        var newState = State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed,
        };
        State = newState;
    }
}