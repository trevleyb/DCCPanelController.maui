using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel;

public partial class Route : DccTable {
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    /// <summary>
    ///     Represents a Turnout with its current state.
    ///     This is controlled by data that comes in via the Withrottle Interface
    /// </summary>
    public Route() { }

    public event EventHandler<RouteStateEnum>? StateChanged;
    partial void OnStateChanged(RouteStateEnum value) => StateChanged?.Invoke(this, value);

    public void ToggleState() {
        var newState = State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            _                       => RouteStateEnum.Active,
        };
        State = newState;
    }
}