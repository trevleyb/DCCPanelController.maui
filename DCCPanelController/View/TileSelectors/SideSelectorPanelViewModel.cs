using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;

namespace DCCPanelController.View.TileSelectors;

public class SideSelectorPanelViewModel : TileSelectorViewModel {
    public SideSelectorPanelViewModel() => AppStateService.Instance.SelectedTileCleared += InstanceOnSelectedTileCleared;

    public ITile? SelectedTile {
        get;
        set {
            Console.WriteLine($"SidePalette: Selected Tile Changed: {field?.Entity.EntityName} -> {value?.Entity.EntityName}");
            if (field == value) return;
            field = value;
            AppStateService.Instance.SelectedTile = value;
            OnPropertyChanged();
        }
    }

    protected override void AfterBuildAllTiles() { }

    private void InstanceOnSelectedTileCleared() => SelectedTile = null;
}