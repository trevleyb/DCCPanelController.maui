using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {DccAddress}")]
public partial class Signal : DccTable {
    [ObservableProperty] private string  _aspect = "Off";

    public event EventHandler<string>? AspectChanged;
    partial void OnAspectChanged(string value) => AspectChanged?.Invoke(this, value);

}