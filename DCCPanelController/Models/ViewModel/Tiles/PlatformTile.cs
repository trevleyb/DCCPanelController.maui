using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class PlatformTile : TrackTile {
    public PlatformTile(PlatformEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }
    
    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("platform", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("platform", Entity.Rotation, SymbolScaleFactor);
    }
}