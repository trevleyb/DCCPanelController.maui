using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackRightTurnoutTile : TurnoutTile {
    public TrackRightTurnoutTile(RightTurnoutEntity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("RightTurnout", Entity.Rotation);
}