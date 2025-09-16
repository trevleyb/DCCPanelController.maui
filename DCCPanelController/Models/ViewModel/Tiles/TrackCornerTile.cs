using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackCornerTile : TrackTile {
    public TrackCornerTile(CornerEntity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("corner", Entity.Rotation);
}