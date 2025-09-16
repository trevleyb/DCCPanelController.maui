using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.Models.ViewModel.Tiles;

public class TrackCrossingTile : TrackTile {
    public TrackCrossingTile(CrossingEntity entity, double gridSize) : base(entity, gridSize) { }
    protected override Microsoft.Maui.Controls.View? CreateTile() => CreateTrackTile("cross", Entity.Rotation);
}