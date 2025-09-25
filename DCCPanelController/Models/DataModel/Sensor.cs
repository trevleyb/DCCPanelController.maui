using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Sensor : ObservableObject, ITable {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private bool    _isEditable;
    [ObservableProperty] private bool    _isModified;
    [ObservableProperty] private int     _dccAddress;
    [ObservableProperty] private bool    _state;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Sensor() => _dccAddress = 0;

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";
    
    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);

    public void ToggleState() {
        State = !State;
    }

}