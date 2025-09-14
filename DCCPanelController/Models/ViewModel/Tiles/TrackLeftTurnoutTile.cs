using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackLeftTurnoutTile : TurnoutTile {
    public TrackLeftTurnoutTile(LeftTurnoutEntity entity, double gridSize, TileDisplayMode displayMode = TileDisplayMode.Normal) : base(entity, gridSize, displayMode) { }

    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("LeftTurnout", Entity.Rotation);

    protected override Microsoft.Maui.Controls.View? CreateSymbol() => CreateTrackTile("leftturnoutunknown", Entity.Rotation, SymbolScaleFactor);
}