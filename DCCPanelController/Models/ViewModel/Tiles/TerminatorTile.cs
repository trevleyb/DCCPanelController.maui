using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TerminatorTile : TrackTile {
    public TerminatorTile(TerminatorEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("terminator", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("terminator", Entity.Rotation, SymbolScaleFactor);
    }
}