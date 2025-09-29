using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {DccAddress}")]
public partial class Signal : DccDccTable {
    [ObservableProperty] private string  _aspect = "Off";

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";
    
    public event EventHandler<string>? AspectChanged;
    partial void OnAspectChanged(string value) => AspectChanged?.Invoke(this, value);

}