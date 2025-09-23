using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.Services;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanelViewModel : TileSelectorViewModel {
    [ObservableProperty] private string _selectedCategory = string.Empty;

    public PillSelectorPanelViewModel() {
        SelectedTile = null;
        AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;
    }

    public ObservableCollection<ITile> TilesForSelectedCategory {
        get {
            if (string.IsNullOrEmpty(SelectedCategory) || !ByCategory.ContainsKey(SelectedCategory)) return[];
            return ByCategory[SelectedCategory];
        }
    }

    public ITile? SelectedTile {
        get;
        set {
            AppStateService.Instance.SelectedTile = value;
            field = value;
            OnPropertyChanged();
        }
    }

    private void InstanceOnSelectedTileCleared() => SelectedTile = null;

    partial void OnSelectedCategoryChanged(string value) => OnPropertyChanged(nameof(TilesForSelectedCategory));

    protected override void AfterBuildAllTiles() { }
}