using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Signal: {Id}: {Name} @  {DccAddress} => State: {State}")]

public partial class Signal : Accessory, IAccessory {
    [ObservableProperty] private string  _aspect = "Off";

    public event EventHandler<string>? AspectChanged;
    partial void OnAspectChanged(string value) => AspectChanged?.Invoke(this, value);

}