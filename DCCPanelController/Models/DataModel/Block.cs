using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Block : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _sensor;
    [ObservableProperty] private bool _isEditable = false;
    [ObservableProperty] private bool _isModified = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    public string DisplayFormat => $"{Name} ({Id})";
    public string State => IsOccupied ? "Occupied" : "Free";
}