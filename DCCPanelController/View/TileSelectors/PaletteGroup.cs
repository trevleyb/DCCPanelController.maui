using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.View.TileSelectors;

public sealed class Palette(Panel panel) : ObservableCollection<PaletteGroup>, INotifyPropertyChanged {
    public Panel Panel { get; init; } = panel;
}

public sealed class PaletteGroup : ObservableCollection<PaletteItem>, INotifyPropertyChanged {
    private readonly List<PaletteItem> _allItems = new();
    public string Category { get; }
    public bool IsExpanded { get; private set; }

    public void AddTile(ITile tile) {
        var item = new PaletteItem(tile, this);
        _allItems.Add(item);
        Add(item);
    }

    public PaletteGroup(string category) {
        Category = category;
        IsExpanded = true;
        ToggleExpandCommand = new RelayCommand(ToggleExpanded);
    }

    public ICommand ToggleExpandCommand { get; }

    private void ToggleExpanded() {
        IsExpanded = !IsExpanded;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));

        if (IsExpanded) {
            foreach (var item in _allItems) {
                Add(item);
            }
        } else {
            Clear();
        }
    }
}

public sealed partial class PaletteItem : ObservableObject {
    public PaletteItem(ITile tile, PaletteGroup group) {
        Tile = tile;
        IsSelected = false;
    }

    [ObservableProperty] private ITile _tile;
    [ObservableProperty] private bool  _isSelected;
}