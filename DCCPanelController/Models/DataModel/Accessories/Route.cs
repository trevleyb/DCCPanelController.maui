using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Route: {Id}: {Name} @  {DccAddress} => State: {State}")]
public partial class Route : Accessory, IAccessory {
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

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