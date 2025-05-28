using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCCommon.Common;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Signal : ObservableObject {
    [ObservableProperty] private string _aspect = "Off";
    [ObservableProperty] private string _dccAddress;

    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private string? _name;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Signal() {
        _dccAddress = string.Empty;
    }
}