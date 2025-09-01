using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Block : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _sensor;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private bool _isModified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    [JsonIgnore]
    public string DisplayFormat => $"{Name} ({Id})";

    [JsonIgnore]
    public string State => IsOccupied ? "Occupied" : "Free";
}