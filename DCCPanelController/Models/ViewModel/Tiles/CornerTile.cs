using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class CornerTile : TrackTile {
    public CornerTile(CornerEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("corner", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("corner", Entity.Rotation, SymbolScaleFactor);
    }
}