using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;

namespace DCCPanelController.View.TileSelectors;

public class SideSelectorPanelViewModel : TileSelectorViewModel {
    protected override void AfterBuildAllTiles() { }
    public ITile? SelectedTile {
        get;
        set {
            AppState.Instance.SelectedTile = value;
            field = value;
        }
    }

}