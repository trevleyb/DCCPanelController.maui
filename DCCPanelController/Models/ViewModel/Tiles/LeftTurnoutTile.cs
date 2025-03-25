using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.ImageManager;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class LeftTurnoutTile : TurnoutTile {
    
    public LeftTurnoutTile(LeftTurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() {
        return CreateTrackTile("LeftTurnout", Entity.Rotation);
    }

    protected override Microsoft.Maui.Controls.View? CreateSymbol() {
        return CreateTrackTile("leftturnoutunknown", Entity.Rotation, SymbolScaleFactor);
    }

}