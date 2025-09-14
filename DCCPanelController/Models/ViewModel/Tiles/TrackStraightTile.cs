using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackStraightTile : TrackTile {
    public TrackStraightTile(StraightEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("straight", Entity.Rotation);

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("straight", Entity.Rotation, SymbolScaleFactor);
}