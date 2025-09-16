using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackLeftTurnoutTile : TurnoutTile {
    public TrackLeftTurnoutTile(LeftTurnoutEntity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("LeftTurnout", Entity.Rotation);
}