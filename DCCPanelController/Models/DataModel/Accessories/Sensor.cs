using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Sensor : DccTable, IDccTable {
    [ObservableProperty] private bool    _state;

    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);

    public void ToggleState() {
        State = !State;
    }

}