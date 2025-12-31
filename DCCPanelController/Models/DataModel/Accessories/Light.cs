using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Light: {Id}: {Name} @  {DccAddress} => State: {State} => {Source}")]
public partial class Light : Accessory, IAccessory {
    [ObservableProperty] private bool _state;
    
    public event EventHandler<bool>? StateChanged;
    partial void OnStateChanged(bool value) => StateChanged?.Invoke(this, value);

    [JsonIgnore] public string DisplayFormat => $"{(Id ?? "Unnamed")} : {Name} ({(DccAddress.HasValue ? DccAddress.Value.ToString() : "—")})";

    public void ToggleState() {
        State = !State;
    }

}