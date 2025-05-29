using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Models.DataModel;

public partial class Block : ObservableObject {
    [ObservableProperty] private string? _id;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(State))]
    private bool _isOccupied;

    [ObservableProperty] private string? _name;

    public string State => IsOccupied ? "Occupied" : "Free";
}