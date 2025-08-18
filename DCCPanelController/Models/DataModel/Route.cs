using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public partial class Route : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
[ObservableProperty] private bool _isEditable = false;
    [ObservableProperty] private bool _isModified = false;
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    public string DisplayFormat => $"{Name} ({Id})";

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }
}