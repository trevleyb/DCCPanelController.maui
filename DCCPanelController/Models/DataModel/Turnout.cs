using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Turnout with its current state.
///     This is controlled by data that comes in via the Withrottle Interface
/// </summary>
[DebuggerDisplay("UniqueId: {Id}, SystemName: {Name}, State: {State}")]
public partial class Turnout : ObservableObject {
    [ObservableProperty] private int _dccAddress;
    [ObservableProperty] private TurnoutStateEnum _default = TurnoutStateEnum.Unknown;
    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private bool _isModified;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private TurnoutStateEnum _state = TurnoutStateEnum.Unknown;
    public string DisplayFormat => $"{Name} ({Id})";
}