using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Sensor: {Id}: {Name} @  {DccAddress} => State: {State}")]
public partial class Sensor : Accessory, IAccessory {
    [ObservableProperty] private bool    _state;

    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);

    public void ToggleState() {
        State = !State;
    }

}