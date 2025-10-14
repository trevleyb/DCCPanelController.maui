using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;

namespace DCCPanelController.View.TileSelectors;

public partial class SideSelectorPanelViewModel : ObservableObject {
    public SideSelectorPanelViewModel() => AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    public Palette Palette { get; set; } = PaletteCache.Palette;
    
    [ObservableProperty] private Dictionary<string, ObservableCollection<ITile>> _byCategory = [];
    [ObservableProperty] private ObservableCollection<string>                    _categories = [];

    public ITile? SelectedTile {
        get;
        set {
            if (field == value) return;
            field = value;
            AppStateService.Instance.SelectedTile = value;
            OnPropertyChanged();
        }
    }

    private void InstanceOnSelectedTileCleared() => SelectedTile = null;
}