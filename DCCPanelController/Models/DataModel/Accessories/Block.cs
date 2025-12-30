using System.Diagnostics;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel.Accessories;

[DebuggerDisplay("Block: {Id}: {Name} @  {DccAddress} => State: {State} => {Source}")]
public partial class Block : Accessory, IAccessory {
    [ObservableProperty] private string? _sensor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    [JsonIgnore]
    public string State => IsOccupied ? "Occupied" : "Free";
    
    public event EventHandler<bool>? StateChanged;
    partial void OnIsOccupiedChanged(bool value) => StateChanged?.Invoke(this, value);
    public void ToggleState() {
        IsOccupied = !IsOccupied;
    }

}