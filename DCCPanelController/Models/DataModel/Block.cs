using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Block : ObservableObject {
    [ObservableProperty] private string? _id;
    [ObservableProperty] private bool _isEditable;
    [ObservableProperty] private bool _isModified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _sensor;

    public string DisplayFormat => $"{Name} ({Id})";
    public string State => IsOccupied ? "Occupied" : "Free";
}