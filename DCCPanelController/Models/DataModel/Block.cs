using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Block : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool    _isEditable;
    [ObservableProperty] private bool    _isModified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _sensor;

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";

    [JsonIgnore]
    public string State => IsOccupied ? "Occupied" : "Free";
    
    public event EventHandler<bool>? StateChanged;
    partial void OnIsOccupiedChanged(bool value) => StateChanged?.Invoke(this, value);
    public void ToggleState() {
        IsOccupied = !IsOccupied;
    }

}