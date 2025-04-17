using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CrossingTile : TrackTile {
    public CrossingTile(CrossingEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("cross", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("cross", Entity.Rotation, SymbolScaleFactor);
    }
}