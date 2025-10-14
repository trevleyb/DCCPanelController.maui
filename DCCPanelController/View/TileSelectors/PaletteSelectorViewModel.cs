using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoreTelephony;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Services;

namespace DCCPanelController.View.TileSelectors;

public partial class PaletteSelectorViewModel : ObservableObject {
    [ObservableProperty] private PaletteDockSide _dockSide = PaletteDockSide.Side;
    
    public PaletteSelectorViewModel() => AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    public Palette Palette { get; set; } = PaletteCache.Palette;

    public ITile? SelectedTile => SelectedItem?.Tile ?? null;

    private PaletteItem? _selectedItem;
    public PaletteItem? SelectedItem {
        get => _selectedItem;
        set {
            if (_selectedItem == value) return;
            _selectedItem?.IsSelected = false;
            _selectedItem = value;
            if (_selectedItem == null) return;
            _selectedItem?.IsSelected = true;
            AppStateService.Instance.SelectedTile = value?.Tile ?? null;
            OnPropertyChanged();
            Console.WriteLine($"Selected Tile->{SelectedItem?.Tile.Entity.EntityName}");
        }
    }

    private void InstanceOnSelectedTileCleared() => SelectedItem = null;
}