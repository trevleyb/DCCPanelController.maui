using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackTerminatorTile : TrackTile {
    public TrackTerminatorTile(TerminatorEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("terminator", Entity.Rotation);

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("terminator", Entity.Rotation, SymbolScaleFactor);
}