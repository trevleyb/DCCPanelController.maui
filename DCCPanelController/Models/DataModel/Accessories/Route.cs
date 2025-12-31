using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Route: {Id}: {Name} @  {DccAddress} => State: {State} => {Source}")]
public partial class Route : Accessory, IAccessory {
    [ObservableProperty] private RouteStateEnum _state = RouteStateEnum.Unknown;

    public event EventHandler<RouteStateEnum>? StateChanged;
    partial void OnStateChanged(RouteStateEnum value) => StateChanged?.Invoke(this, value);

    [JsonIgnore] public string DisplayFormat => $"{(Id ?? "Unnamed")} : {Name} ({(DccAddress.HasValue ? DccAddress.Value.ToString() : "—")})";
    
    public void ToggleState() {
        var newState = State switch {
            RouteStateEnum.Active   => RouteStateEnum.Inactive,
            _                       => RouteStateEnum.Active,
        };
        State = newState;
    }
}