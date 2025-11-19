using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Light: {Id}: {Name} @  {DccAddress} => State: {State}")]
public partial class Light : Accessory, IAccessory {
    [ObservableProperty] private bool _state;
    
    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);
    public void ToggleState() {
        State = !State;
    }

}