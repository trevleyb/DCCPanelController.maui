using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackAngleCrossingTile : TrackTile {
    public TrackAngleCrossingTile(AngleCrossingEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("angle", Entity.Rotation);

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("angle", Entity.Rotation, SymbolScaleFactor);
}