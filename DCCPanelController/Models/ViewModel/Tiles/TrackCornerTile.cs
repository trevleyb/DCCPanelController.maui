using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackCornerTile : TrackTile {
    public TrackCornerTile(CornerEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("corner", Entity.Rotation);

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("corner", Entity.Rotation, SymbolScaleFactor);
}