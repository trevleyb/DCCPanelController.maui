using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Sensor: {Id}: {Name} @  {DccAddress} => State: {State} => {Source}")]
public partial class Sensor : Accessory, IAccessory {
    [ObservableProperty] private bool    _state;

    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);

    [JsonIgnore] public string DisplayFormat => $"{(Id ?? "Unnamed")} : {Name} ({(DccAddress.HasValue ? DccAddress.Value.ToString() : "—")})";

    public void ToggleState() {
        State = !State;
    }

}