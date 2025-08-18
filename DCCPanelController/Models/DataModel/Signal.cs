using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Signal : ObservableObject {
    [ObservableProperty] private string _aspect = "Off";
    [ObservableProperty] private int _dccAddress;
    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isEditable = false;
    [ObservableProperty] private bool _isModified = false;
    [ObservableProperty] private string? _name;

    public string DisplayFormat => $"{Name} ({Id})";

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Signal() {
        _dccAddress = 0;
    }
}