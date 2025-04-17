using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class RightTurnoutTile : TurnoutTile {
    public RightTurnoutTile(RightTurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("RightTurnout", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("rightturnoutunknown", Entity.Rotation, SymbolScaleFactor);
    }
}