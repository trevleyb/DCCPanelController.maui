using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Sensor : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private bool _isEditable = false;
    [ObservableProperty] private bool _isModified = false;
    [ObservableProperty] private bool _state;
    [ObservableProperty] private int _dccAddress;

    public string DisplayFormat => $"{Name} ({Id})";

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Sensor() {
        _dccAddress = 0;
    }
}