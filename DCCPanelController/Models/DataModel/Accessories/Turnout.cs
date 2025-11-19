using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Turnout: {Id}: {Name} @  {DccAddress} => State: {State}")]
public partial class Turnout : Accessory, IAccessory {
    [ObservableProperty] private TurnoutStateEnum _state   = TurnoutStateEnum.Unknown;
    [ObservableProperty] private TurnoutStateEnum _default = TurnoutStateEnum.Unknown;

    public event EventHandler<TurnoutStateEnum>? StateChanged;
    partial void OnStateChanged(TurnoutStateEnum value) => StateChanged?.Invoke(this, value);
   
    public void ToggleState() {
        var newState = State switch {
            TurnoutStateEnum.Closed => TurnoutStateEnum.Thrown,
            TurnoutStateEnum.Thrown => TurnoutStateEnum.Closed,
            _                       => TurnoutStateEnum.Closed,
        };
        State = newState;
    }
}